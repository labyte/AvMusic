using AvMusic.Core.Models;

namespace AvMusic.Core.Playback;

/// <summary>
/// 跨平台音频播放引擎抽象。
/// </summary>
public interface IPlaybackEngine : IAsyncDisposable
{
    PlaybackState State { get; }

    /// <summary>底层播放器是否正在播放（非仅 UI 状态）。</summary>
    bool IsPlaying { get; }

    event EventHandler<PlaybackState>? StateChanged;

    /// <summary>当前曲目自然播放结束。</summary>
    event EventHandler? PlaybackEnded;

    /// <summary>切换曲目时立即更新 UI（下载/解码完成前处于 Loading）。</summary>
    Task BeginLoadAsync(Song song, CancellationToken cancellationToken = default);

    Task LoadAsync(Song song, Uri streamUri, CancellationToken cancellationToken = default);

    /// <summary>加载或播放失败时更新错误状态（保留当前曲目信息）。</summary>
    Task NotifyErrorAsync(Song song, string message, CancellationToken cancellationToken = default);

    Task PlayAsync(CancellationToken cancellationToken = default);

    Task PauseAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default);

    Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default);
}
