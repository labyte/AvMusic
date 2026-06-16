using AvMusic.Core.Models;

namespace AvMusic.Core.Playback;

public sealed class PlaybackQueue : IPlaybackQueue
{
    private readonly Random _random = new();
    private List<Song> _songs = [];

    public IReadOnlyList<Song> Songs => _songs;

    public int CurrentIndex { get; private set; }

    public Song? Current => _songs.Count == 0 || CurrentIndex < 0 || CurrentIndex >= _songs.Count
        ? null
        : _songs[CurrentIndex];

    public PlaybackMode Mode { get; set; } = PlaybackMode.Sequential;

    public event EventHandler? QueueChanged;

    public void SetQueue(IReadOnlyList<Song> songs, int startIndex = 0)
    {
        _songs = songs.ToList();
        CurrentIndex = _songs.Count == 0 ? -1 : Math.Clamp(startIndex, 0, _songs.Count - 1);
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void InsertNext(Song song)
    {
        if (_songs.Count == 0)
        {
            SetQueue([song], 0);
            return;
        }

        var insertAt = CurrentIndex >= 0 ? CurrentIndex + 1 : _songs.Count;
        _songs.Insert(insertAt, song);
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AppendSongs(IReadOnlyList<Song> songs)
    {
        if (songs.Count == 0)
        {
            return;
        }

        var hadCurrent = CurrentIndex >= 0;
        _songs.AddRange(songs);

        if (!hadCurrent && _songs.Count > 0)
        {
            CurrentIndex = 0;
        }

        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _songs = [];
        CurrentIndex = -1;
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _songs.Count)
        {
            return;
        }

        var wasCurrent = index == CurrentIndex;
        _songs.RemoveAt(index);

        if (_songs.Count == 0)
        {
            CurrentIndex = -1;
        }
        else if (index < CurrentIndex)
        {
            CurrentIndex--;
        }
        else if (wasCurrent && CurrentIndex >= _songs.Count)
        {
            CurrentIndex = _songs.Count - 1;
        }

        QueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool SetCurrentIndex(int index)
    {
        if (index < 0 || index >= _songs.Count)
        {
            return false;
        }

        CurrentIndex = index;
        QueueChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public Song? MoveNext()
    {
        if (_songs.Count == 0)
        {
            return null;
        }

        switch (Mode)
        {
            case PlaybackMode.RepeatOne:
                return Current;

            case PlaybackMode.Shuffle:
                if (_songs.Count == 1)
                {
                    return Current;
                }

                var nextIndex = CurrentIndex;
                while (nextIndex == CurrentIndex)
                {
                    nextIndex = _random.Next(_songs.Count);
                }

                CurrentIndex = nextIndex;
                return Current;

            case PlaybackMode.RepeatAll:
                CurrentIndex = (CurrentIndex + 1) % _songs.Count;
                return Current;

            default:
                if (CurrentIndex + 1 >= _songs.Count)
                {
                    return null;
                }

                CurrentIndex++;
                return Current;
        }
    }

    public Song? MovePrevious()
    {
        if (_songs.Count == 0)
        {
            return null;
        }

        switch (Mode)
        {
            case PlaybackMode.RepeatOne:
                return Current;

            case PlaybackMode.Shuffle:
            case PlaybackMode.RepeatAll:
                CurrentIndex = CurrentIndex <= 0 ? _songs.Count - 1 : CurrentIndex - 1;
                return Current;

            default:
                if (CurrentIndex <= 0)
                {
                    return null;
                }

                CurrentIndex--;
                return Current;
        }
    }
}
