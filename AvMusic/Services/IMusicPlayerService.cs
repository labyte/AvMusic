using AvMusic.Core.Models;
using AvMusic.Core.Playback;

namespace AvMusic.Services;

/// <summary>
/// 音乐播放编排：队列、群晖流 URL、播放引擎。
/// </summary>
public interface IMusicPlayerService
{
    IPlaybackEngine Engine { get; }

    IPlaybackQueue Queue { get; }

    PlaybackMode Mode { get; set; }

    /// <summary>播放单首（清空队列仅保留此曲）。</summary>
    Task PlaySongAsync(Song song, CancellationToken cancellationToken = default);

    /// <summary>设置队列并从指定位置开始播放。</summary>
    Task PlayQueueAsync(
        IReadOnlyList<Song> songs,
        int startIndex = 0,
        CancellationToken cancellationToken = default);

    Task PlayAsync(CancellationToken cancellationToken = default);

    Task PauseAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task PlayNextAsync(CancellationToken cancellationToken = default);

    Task PlayPreviousAsync(CancellationToken cancellationToken = default);

    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default);

    Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default);

    Uri? GetCoverUri(string songId);
}
