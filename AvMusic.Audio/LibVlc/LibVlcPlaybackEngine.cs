using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using AvMusic.Core.Threading;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;

namespace AvMusic.Audio.LibVlc;

/// <summary>
/// 基于 LibVLC 的跨平台播放引擎（Desktop 优先）。
/// </summary>
public sealed class LibVlcPlaybackEngine : IPlaybackEngine
{
    private readonly LibVlcHost _host;
    private readonly IMainThreadDispatcher _ui;
    private readonly ILogger<LibVlcPlaybackEngine> _logger;
    private readonly MediaPlayer _player;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private Media? _media;
    private PlaybackState _state = new();
    private Song? _currentSong;
    private Timer? _positionSyncTimer;

    public LibVlcPlaybackEngine(
        LibVlcHost host,
        IMainThreadDispatcher ui,
        ILogger<LibVlcPlaybackEngine> logger)
    {
        _host = host;
        _ui = ui;
        _logger = logger;
        _player = new MediaPlayer(_host.Instance);

        _player.TimeChanged += OnTimeChanged;
        _player.LengthChanged += OnLengthChanged;
        _player.Playing += OnPlaying;
        _player.Paused += OnPaused;
        _player.Stopped += OnStopped;
        _player.EndReached += OnEndReached;
        _player.EncounteredError += OnEncounteredError;
    }

    public PlaybackState State => _state;

    public bool IsPlaying => _player.IsPlaying;

    public event EventHandler<PlaybackState>? StateChanged;

    public event EventHandler? PlaybackEnded;

    public Task BeginLoadAsync(Song song, CancellationToken cancellationToken = default)
    {
        return _ui.InvokeAsync(() =>
        {
            StopPositionSync();
            _media?.Dispose();
            _media = null;
            _player.Stop();
            _player.Media = null;
            _currentSong = song;

            var duration = song.DurationSeconds > 0
                ? TimeSpan.FromSeconds(song.DurationSeconds)
                : TimeSpan.Zero;

            UpdateState(new PlaybackState
            {
                Status = PlaybackStatus.Loading,
                CurrentSong = song,
                Position = TimeSpan.Zero,
                Duration = duration,
                ErrorMessage = null
            });

            return Task.CompletedTask;
        });
    }

    public Task NotifyErrorAsync(Song song, string message, CancellationToken cancellationToken = default)
    {
        return _ui.InvokeAsync(() =>
        {
            StopPositionSync();
            _currentSong = song;
            UpdateState(_state with
            {
                Status = PlaybackStatus.Error,
                CurrentSong = song,
                ErrorMessage = message
            });

            return Task.CompletedTask;
        });
    }

    public async Task LoadAsync(Song song, Uri streamUri, CancellationToken cancellationToken = default)
    {
        await _loadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            StopPositionSync();
            _currentSong = song;
            _logger.LogInformation("加载流 {Title}: {Uri}", song.Title, streamUri);

            var duration = song.DurationSeconds > 0
                ? TimeSpan.FromSeconds(song.DurationSeconds)
                : TimeSpan.Zero;

            UpdateState(_state with
            {
                Status = PlaybackStatus.Loading,
                CurrentSong = song,
                Position = TimeSpan.Zero,
                Duration = duration,
                ErrorMessage = null
            });

            await _ui.InvokeAsync(() =>
            {
                _media?.Dispose();
                _player.Stop();
                _media = CreateMedia(streamUri);
                _player.Media = _media;
                return Task.CompletedTask;
            }).ConfigureAwait(false);

            var mediaForParse = _media;
            var isLocalFile = streamUri.IsFile;
            if (mediaForParse is not null)
            {
                _ = Task.Run(() => TryParseDurationAsync(mediaForParse, song, isLocalFile), cancellationToken);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        _ui.Post(() =>
        {
            if (_player.Media is null)
            {
                _logger.LogWarning("Play 被调用但 Media 为空");
                return;
            }

            _player.Play();
            StartPositionSync();
            SyncStateFromPlayer();
        });
        return Task.CompletedTask;
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        _ui.Post(() =>
        {
            _player.Pause();
            StopPositionSync();
            SyncStateFromPlayer();
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _ui.Post(() =>
        {
            StopPositionSync();
            _player.Stop();
            UpdateState(_state with
            {
                Status = PlaybackStatus.Stopped,
                Position = TimeSpan.Zero
            });
        });
        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        _ui.Post(() =>
        {
            if (_player.Media is not null)
            {
                _player.Time = (long)position.TotalMilliseconds;
                SyncStateFromPlayer();
            }
        });
        return Task.CompletedTask;
    }

    public Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        _ui.Post(() => _player.Volume = (int)(Math.Clamp(volume, 0, 1) * 100));
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        StopPositionSync();
        await _ui.InvokeAsync(() =>
        {
            _player.TimeChanged -= OnTimeChanged;
            _player.LengthChanged -= OnLengthChanged;
            _player.Playing -= OnPlaying;
            _player.Paused -= OnPaused;
            _player.Stopped -= OnStopped;
            _player.EndReached -= OnEndReached;
            _player.EncounteredError -= OnEncounteredError;

            _player.Stop();
            _media?.Dispose();
            _player.Dispose();
            return Task.CompletedTask;
        }).ConfigureAwait(false);

        _loadLock.Dispose();
    }

    private void StartPositionSync()
    {
        StopPositionSync();
        _positionSyncTimer = new Timer(
            _ => _ui.Post(SyncStateFromPlayer),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(300));
    }

    private void StopPositionSync()
    {
        _positionSyncTimer?.Dispose();
        _positionSyncTimer = null;
    }

    private void SyncStateFromPlayer()
    {
        if (_player.Media is null || _currentSong is null)
        {
            return;
        }

        var lengthMs = _player.Length;
        var duration = lengthMs > 0
            ? TimeSpan.FromMilliseconds(lengthMs)
            : _state.Duration;

        if (duration <= TimeSpan.Zero && _currentSong.DurationSeconds > 0)
        {
            duration = TimeSpan.FromSeconds(_currentSong.DurationSeconds);
        }

        var status = _player.IsPlaying
            ? PlaybackStatus.Playing
            : PlaybackStatus.Paused;

        UpdateState(_state with
        {
            Status = status,
            CurrentSong = _currentSong,
            Position = TimeSpan.FromMilliseconds(Math.Max(0, _player.Time)),
            Duration = duration
        });
    }

    private Media CreateMedia(Uri streamUri)
    {
        if (streamUri.IsFile)
        {
            return new Media(_host.Instance, streamUri.LocalPath, FromType.FromPath);
        }

        var media = new Media(_host.Instance, streamUri.AbsoluteUri, FromType.FromLocation);
        media.AddOption(":network-caching=3000");
        media.AddOption(":http-reconnect");
        media.AddOption(":http-ssl-insecure");
        return media;
    }

    private async Task TryParseDurationAsync(Media media, Song song, bool isLocalFile)
    {
        try
        {
            if (isLocalFile)
            {
                if (media.Duration > 0)
                {
                    UpdateState(_state with
                    {
                        Duration = TimeSpan.FromMilliseconds(media.Duration)
                    });
                }

                return;
            }

            var parsed = await WaitForParsedAsync(media, TimeSpan.FromSeconds(5), CancellationToken.None)
                .ConfigureAwait(false);

            if (parsed && media.Duration > 0)
            {
                UpdateState(_state with { Duration = TimeSpan.FromMilliseconds(media.Duration) });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "解析时长失败: {Title}", song.Title);
        }
    }

    private static async Task<bool> WaitForParsedAsync(
        Media media,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (media.IsParsed)
        {
            return true;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? sender, MediaParsedChangedEventArgs args)
        {
            if (args.ParsedStatus is MediaParsedStatus.Done or MediaParsedStatus.Failed)
            {
                media.ParsedChanged -= Handler;
                tcs.TrySetResult(args.ParsedStatus == MediaParsedStatus.Done);
            }
        }

        media.ParsedChanged += Handler;
        media.Parse(MediaParseOptions.ParseLocal, timeout: (int)timeout.TotalMilliseconds);

        using var timeoutCts = new CancellationTokenSource(timeout);
        await using var timeoutRegistration = timeoutCts.Token.Register(() => tcs.TrySetResult(false));

        return await tcs.Task.ConfigureAwait(false);
    }

    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e) =>
        UpdateState(_state with { Position = TimeSpan.FromMilliseconds(e.Time) });

    private void OnLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e) =>
        UpdateState(_state with { Duration = TimeSpan.FromMilliseconds(e.Length) });

    private void OnPlaying(object? sender, EventArgs e)
    {
        StartPositionSync();
        UpdateState(_state with { Status = PlaybackStatus.Playing, ErrorMessage = null });
    }

    private void OnPaused(object? sender, EventArgs e)
    {
        StopPositionSync();
        UpdateState(_state with { Status = PlaybackStatus.Paused });
    }

    private void OnStopped(object? sender, EventArgs e)
    {
        StopPositionSync();
        UpdateState(_state with { Status = PlaybackStatus.Stopped });
    }

    private void OnEndReached(object? sender, EventArgs e)
    {
        StopPositionSync();
        UpdateState(_state with { Status = PlaybackStatus.Stopped, Position = _state.Duration });
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnEncounteredError(object? sender, EventArgs e)
    {
        StopPositionSync();
        _logger.LogError("LibVLC 播放出错: {Title}", _currentSong?.Title);
        UpdateState(_state with
        {
            Status = PlaybackStatus.Error,
            ErrorMessage = "播放失败，请重试或更换歌曲"
        });
    }

    private void UpdateState(PlaybackState state)
    {
        _state = state;
        StateChanged?.Invoke(this, state);
    }
}
