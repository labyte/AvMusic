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

    void Clear();

    /// <summary>移动到下一首并返回；若无法继续则返回 null。</summary>
    Song? MoveNext();

    /// <summary>移动到上一首并返回；若已在开头则返回 null。</summary>
    Song? MovePrevious();
}
