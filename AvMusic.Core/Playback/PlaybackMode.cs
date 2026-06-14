namespace AvMusic.Core.Playback;

/// <summary>
/// 播放队列模式。
/// </summary>
public enum PlaybackMode
{
    /// <summary>顺序播放，列表结束后停止。</summary>
    Sequential,

    /// <summary>列表循环。</summary>
    RepeatAll,

    /// <summary>单曲循环。</summary>
    RepeatOne,

    /// <summary>随机播放。</summary>
    Shuffle
}
