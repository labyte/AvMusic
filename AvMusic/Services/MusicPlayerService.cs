using System.Net.Http;
using System.Text;
using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using AvMusic.Core.Session;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Streaming;
using Microsoft.Extensions.Logging;

namespace AvMusic.Services;

public sealed class MusicPlayerService : IMusicPlayerService
{
    private readonly ISessionState _session;
    private readonly ISynologyConnectionContext _connection;
    private readonly StreamUrlBuilder _streamUrlBuilder;
    private readonly SynologyStreamDownloader _streamDownloader;
    private readonly ILogger<MusicPlayerService> _logger;
    private readonly object _playGate = new();
    private CancellationTokenSource? _playCts;
    private int _playGeneration;
    private string? _localCachePath;

    public MusicPlayerService(
        ISessionState session,
        ISynologyConnectionContext connection,
        StreamUrlBuilder streamUrlBuilder,
        SynologyStreamDownloader streamDownloader,
        IPlaybackEngine engine,
        IPlaybackQueue queue,
        ILogger<MusicPlayerService> logger)
    {
        _session = session;
        _connection = connection;
        _streamUrlBuilder = streamUrlBuilder;
        _streamDownloader = streamDownloader;
        Engine = engine;
        Queue = queue;
        _logger = logger;

        Engine.PlaybackEnded += OnPlaybackEnded;
    }

    public IPlaybackEngine Engine { get; }

    public IPlaybackQueue Queue { get; }

    public PlaybackMode Mode
    {
        get => Queue.Mode;
        set => Queue.Mode = value;
    }

    public Task PlaySongAsync(Song song, CancellationToken cancellationToken = default)
    {
        Queue.SetQueue([song], 0);
        return RunPlaySessionAsync(LoadAndPlayCurrentAsync);
    }

    public Task PlayQueueAsync(
        IReadOnlyList<Song> songs,
        int startIndex = 0,
        CancellationToken cancellationToken = default)
    {
        Queue.SetQueue(songs, startIndex);
        return RunPlaySessionAsync(LoadAndPlayCurrentAsync);
    }

    public Task PlayAsync(CancellationToken cancellationToken = default) =>
        Engine.PlayAsync(CancellationToken.None);

    public Task PauseAsync(CancellationToken cancellationToken = default) =>
        Engine.PauseAsync(CancellationToken.None);

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        CancelPlaySession();
        return Engine.StopAsync(CancellationToken.None);
    }

    public Task PlayNextAsync(CancellationToken cancellationToken = default) =>
        RunPlaySessionAsync(LoadAndPlayCurrentAsync);

    public async Task PlayPreviousAsync(CancellationToken cancellationToken = default)
    {
        if (Engine.State.Position > TimeSpan.FromSeconds(3))
        {
            await Engine.SeekAsync(TimeSpan.Zero, CancellationToken.None).ConfigureAwait(true);
            return;
        }

        if (Queue.MovePrevious() is null)
        {
            await Engine.SeekAsync(TimeSpan.Zero, CancellationToken.None).ConfigureAwait(true);
            return;
        }

        await RunPlaySessionAsync(LoadAndPlayCurrentAsync).ConfigureAwait(true);
    }

    public Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default) =>
        Engine.SeekAsync(position, CancellationToken.None);

    public Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default) =>
        Engine.SetVolumeAsync(volume, CancellationToken.None);

    public Uri? GetCoverUri(string songId)
    {
        var sid = _session.SessionId;
        return sid is null ? null : _streamUrlBuilder.BuildCoverUri(songId, sid);
    }

    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        try
        {
            await PlayNextAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动播放下一首失败");
        }
    }

    private async Task RunPlaySessionAsync(Func<CancellationToken, Task> play)
    {
        int generation;
        CancellationToken token;
        lock (_playGate)
        {
            _playCts?.Cancel();
            _playCts?.Dispose();
            _playCts = new CancellationTokenSource();
            token = _playCts.Token;
            generation = ++_playGeneration;
        }

        try
        {
            await play(token).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            _logger.LogDebug("播放会话已取消");
        }
        catch (Exception ex) when (generation != _playGeneration)
        {
            _logger.LogDebug(ex, "过期的播放会话失败，已忽略");
        }
        catch (Exception ex)
        {
            var song = Queue.Current;
            if (song is not null)
            {
                await Engine.NotifyErrorAsync(song, FormatUserError(ex), CancellationToken.None)
                    .ConfigureAwait(true);
            }

            throw;
        }
    }

    private void CancelPlaySession()
    {
        lock (_playGate)
        {
            _playCts?.Cancel();
            _playCts?.Dispose();
            _playCts = null;
        }
    }

    private void SyncConnectionFromSession()
    {
        if (_session.Server is not null)
        {
            _connection.Server = _session.Server;
        }
    }

    private async Task LoadAndPlayCurrentAsync(CancellationToken cancellationToken)
    {
        CleanupLocalCache();
        SyncConnectionFromSession();

        var song = Queue.Current
            ?? throw new InvalidOperationException("播放队列为空。");

        // 立刻切换 UI 到目标曲目，下载完成后再 Load/Play
        await Engine.BeginLoadAsync(song, cancellationToken).ConfigureAwait(true);

        var sid = _session.SessionId
            ?? throw new InvalidOperationException("尚未登录，无法播放。");

        var candidates = _streamUrlBuilder.BuildPlaybackCandidates(song, sid);
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("无法生成播放地址，请重新登录后再试。");
        }

        var errors = new StringBuilder();

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var download = await _streamDownloader
                .DownloadAsync(candidate.Uri, song.Id, candidate.PreferredExtension, cancellationToken)
                .ConfigureAwait(false);

            if (download.FilePath is null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                errors.AppendLine(FormatDownloadError(candidate.Uri, download));
                continue;
            }

            try
            {
                var fileUri = new Uri(Path.GetFullPath(download.FilePath));
                if (await TryLoadAndPlayLocalAsync(song, fileUri).ConfigureAwait(true))
                {
                    _localCachePath = download.FilePath;
                    _logger.LogInformation("播放成功 {Title}", song.Title);
                    return;
                }

                errors.AppendLine($"已下载但 LibVLC 无法播放: {download.FilePath}");
            }
            finally
            {
                if (_localCachePath != download.FilePath)
                {
                    _streamDownloader.Delete(download.FilePath);
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new InvalidOperationException(
            $"无法播放「{song.Title}」。{errors}".Trim());
    }

    private static string FormatUserError(Exception ex) =>
        ex switch
        {
            HttpRequestException => "网络请求失败，请检查 NAS 连接或在登录页勾选「信任证书」。",
            InvalidOperationException => ex.Message,
            _ => ex.Message
        };

    private async Task<bool> TryLoadAndPlayLocalAsync(Song song, Uri fileUri)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnStateChanged(object? sender, PlaybackState state)
        {
            if (state.CurrentSong?.Id != song.Id)
            {
                return;
            }

            if (state.Status == PlaybackStatus.Playing)
            {
                tcs.TrySetResult(true);
            }
            else if (state.Status == PlaybackStatus.Error)
            {
                tcs.TrySetResult(false);
            }
        }

        Engine.StateChanged += OnStateChanged;
        try
        {
            await Engine.LoadAsync(song, fileUri, CancellationToken.None).ConfigureAwait(true);
            await Engine.PlayAsync(CancellationToken.None).ConfigureAwait(true);

            var finished = await Task
                .WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(20)))
                .ConfigureAwait(true);

            if (finished == tcs.Task && tcs.Task.IsCompletedSuccessfully)
            {
                return tcs.Task.Result;
            }

            return Engine.IsPlaying;
        }
        finally
        {
            Engine.StateChanged -= OnStateChanged;
        }
    }

    private static string FormatDownloadError(Uri uri, StreamDownloadResult result)
    {
        var status = result.HttpStatusCode?.ToString() ?? "N/A";
        var detail = result.ErrorDetail ?? "未知";
        return $"[{status}] {detail} ({uri.AbsolutePath})";
    }

    private void CleanupLocalCache()
    {
        if (_localCachePath is null)
        {
            return;
        }

        _streamDownloader.Delete(_localCachePath);
        _localCachePath = null;
    }
}
