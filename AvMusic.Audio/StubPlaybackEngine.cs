using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using Microsoft.Extensions.Logging;

namespace AvMusic.Audio;

/// <summary>
/// 占位播放引擎（阶段 2 将替换为 LibVLC 等实现）。
/// </summary>
public sealed class StubPlaybackEngine : IPlaybackEngine
{
    private readonly ILogger<StubPlaybackEngine> _logger;
    private PlaybackState _state = new();

    public StubPlaybackEngine(ILogger<StubPlaybackEngine> logger)
    {
        _logger = logger;
    }

    public PlaybackState State => _state;

    public bool IsPlaying => _state.Status == PlaybackStatus.Playing;

    public event EventHandler<PlaybackState>? StateChanged;

    public event EventHandler? PlaybackEnded;

    public Task BeginLoadAsync(Song song, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("准备播放 {Title}", song.Title);
        UpdateState(new PlaybackState
        {
            Status = PlaybackStatus.Loading,
            CurrentSong = song,
            Position = TimeSpan.Zero,
            Duration = TimeSpan.FromSeconds(song.DurationSeconds),
            ErrorMessage = null
        });
        return Task.CompletedTask;
    }

    public Task NotifyErrorAsync(Song song, string message, CancellationToken cancellationToken = default)
    {
        UpdateState(_state with
        {
            Status = PlaybackStatus.Error,
            CurrentSong = song,
            ErrorMessage = message
        });
        return Task.CompletedTask;
    }

    public Task LoadAsync(Song song, Uri streamUri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("加载歌曲 {Title}，流地址 {Uri}", song.Title, streamUri);
        UpdateState(new PlaybackState
        {
            Status = PlaybackStatus.Paused,
            CurrentSong = song,
            Duration = TimeSpan.FromSeconds(song.DurationSeconds)
        });
        return Task.CompletedTask;
    }

    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        if (_state.CurrentSong is null)
        {
            return Task.CompletedTask;
        }

        UpdateState(_state with { Status = PlaybackStatus.Playing });
        return Task.CompletedTask;
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        UpdateState(_state with { Status = PlaybackStatus.Paused });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        UpdateState(new PlaybackState { Status = PlaybackStatus.Stopped });
        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        UpdateState(_state with { Position = position });
        return Task.CompletedTask;
    }

    public Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("设置音量 {Volume:P0}", volume);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private void UpdateState(PlaybackState state)
    {
        _state = state;
        StateChanged?.Invoke(this, state);
    }
}
