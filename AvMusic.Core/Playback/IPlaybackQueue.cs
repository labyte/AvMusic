using AvMusic.Core.Models;

namespace AvMusic.Core.Playback;

public interface IPlaybackQueue
{
    IReadOnlyList<Song> Songs { get; }

    int CurrentIndex { get; }

    Song? Current { get; }

    PlaybackMode Mode { get; set; }

    event EventHandler? QueueChanged;

    void SetQueue(IReadOnlyList<Song> songs, int startIndex = 0);

    /// <summary>在当前曲目之后插入下一首播放。</summary>
    void InsertNext(Song song);

    /// <summary>在现有队列末尾追加歌曲。</summary>
    void AppendSongs(IReadOnlyList<Song> songs);

    void Clear();

    /// <summary>从队列中移除指定索引的歌曲。</summary>
    void RemoveAt(int index);

    /// <summary>切换当前播放索引（不改动队列内容）。</summary>
    bool SetCurrentIndex(int index);

    /// <summary>移动到下一首并返回；若无法继续则返回 null。</summary>
    Song? MoveNext();

    /// <summary>移动到上一首并返回；若已在开头则返回 null。</summary>
    Song? MovePrevious();
}
