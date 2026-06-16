using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using AvMusic.Services;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

/// <summary>
/// 歌曲列表行，便于 XAML 绑定播放命令。
/// </summary>
public sealed class SongRowViewModel : ViewModelBase
{
    private int _rating;
    private Bitmap? _cover;
    private bool _isCurrentTrackPlaying;
    private bool _isMenuOpen;
    private bool _isInfoOpen;
    private readonly Action<SongRowViewModel>? _onMenuOpening;
    private readonly Action<SongRowViewModel>? _onMenuClosed;

    public SongRowViewModel(Song song, Func<Song, Task> playAsync)
        : this(song, 0, playAsync, null, null, null, null, null, null, null, null, null, null, null, null)
    {
    }

    public SongRowViewModel(
        Song song,
        int index,
        Func<Song, Task> playAsync,
        Func<Song, int, Task>? toggleFavoriteAsync,
        Func<Song, Task>? openArtistAsync,
        Func<Song, Task>? openAlbumAsync,
        Func<Song, Task>? addToQueueAsync,
        Func<Song, Task>? playNextAsync,
        Func<Song, Task>? togglePlayPauseAsync,
        Func<Song, Task>? downloadAsync,
        Action<SongRowViewModel>? showInfoAsync,
        Action<SongRowViewModel>? onMenuOpening,
        Action<SongRowViewModel>? onMenuClosed,
        ICoverCacheService? coverCache,
        IMusicPlayerService? player)
    {
        Song = song;
        Index = index;
        _rating = song.Rating;
        _onMenuOpening = onMenuOpening;
        _onMenuClosed = onMenuClosed;
        PlayCommand = ReactiveCommand.CreateFromTask(() => playAsync(Song));
        IndexActionCommand = ReactiveCommand.CreateFromTask(() =>
            togglePlayPauseAsync is not null ? togglePlayPauseAsync(Song) : playAsync(Song));

        if (toggleFavoriteAsync is not null)
        {
            ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var newRating = IsFavorite ? 0 : 5;
                await toggleFavoriteAsync(Song, newRating).ConfigureAwait(true);
                _rating = newRating;
                this.RaisePropertyChanged(nameof(IsFavorite));
            });
            MenuToggleFavoriteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                CloseMenu();
                var newRating = IsFavorite ? 0 : 5;
                await toggleFavoriteAsync(Song, newRating).ConfigureAwait(true);
                _rating = newRating;
                this.RaisePropertyChanged(nameof(IsFavorite));
            });
        }

        if (openArtistAsync is not null && CanOpenArtist)
        {
            OpenArtistCommand = ReactiveCommand.CreateFromTask(() => openArtistAsync(Song));
        }

        if (openAlbumAsync is not null && CanOpenAlbum)
        {
            OpenAlbumCommand = ReactiveCommand.CreateFromTask(() => openAlbumAsync(Song));
        }

        if (addToQueueAsync is not null)
        {
            AddToQueueCommand = ReactiveCommand.CreateFromTask(() => addToQueueAsync(Song));
            MenuAddToQueueCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                CloseMenu();
                await addToQueueAsync(Song).ConfigureAwait(true);
            });
        }

        if (playNextAsync is not null)
        {
            MenuPlayNextCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                CloseMenu();
                await playNextAsync(Song).ConfigureAwait(true);
            });
        }

        if (downloadAsync is not null)
        {
            MenuDownloadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                CloseMenu();
                await downloadAsync(Song).ConfigureAwait(true);
            });
        }

        MenuPlayCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CloseMenu();
            await playAsync(Song).ConfigureAwait(true);
        });
        ToggleMenuCommand = ReactiveCommand.Create(ToggleMenu);
        CloseMenuCommand = ReactiveCommand.Create(CloseMenu);

        if (showInfoAsync is not null)
        {
            MenuShowInfoCommand = ReactiveCommand.Create(() =>
            {
                CloseMenu();
                showInfoAsync(this);
            });
        }

        if (coverCache is not null)
        {
            _ = LoadCoverAsync(coverCache);
        }

        _ = player;
    }

    public Song Song { get; }

    public int Index { get; }

    public string IndexText => Index > 0 ? Index.ToString("D2") : string.Empty;

    public string Title => Song.Title;

    public string ArtistName => string.IsNullOrWhiteSpace(Song.Artist) ? "未知艺术家" : Song.Artist;

    public string AlbumName => string.IsNullOrWhiteSpace(Song.Album) ? "—" : Song.Album;

    public string Subtitle => $"{Song.Artist} · {Song.Album}";

    public string DurationText => TimeSpan.FromSeconds(Song.DurationSeconds).ToString(@"m\:ss");

    public bool CanOpenArtist => !string.IsNullOrWhiteSpace(Song.Artist) && Song.Artist != "未知艺术家";

    public bool CanOpenAlbum => !string.IsNullOrWhiteSpace(Song.Album) && Song.Album != "—";

    public bool IsFavorite => _rating >= 5;

    /// <summary>当前行是否为正在播放的歌曲（用于样式类 song-row-playing）。</summary>
    public bool IsCurrentTrackPlaying
    {
        get => _isCurrentTrackPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isCurrentTrackPlaying, value);
    }

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _isMenuOpen, value) && !value)
            {
                _onMenuClosed?.Invoke(this);
            }
        }
    }

    public bool IsInfoOpen
    {
        get => _isInfoOpen;
        set => this.RaiseAndSetIfChanged(ref _isInfoOpen, value);
    }

    public string SongInfoText => BuildSongInfoText();

    public Bitmap? Cover
    {
        get => _cover;
        private set => this.RaiseAndSetIfChanged(ref _cover, value);
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }

    public ReactiveCommand<Unit, Unit> IndexActionCommand { get; }

    public ReactiveCommand<Unit, Unit>? ToggleFavoriteCommand { get; }

    public ReactiveCommand<Unit, Unit>? OpenArtistCommand { get; }

    public ReactiveCommand<Unit, Unit>? OpenAlbumCommand { get; }

    public ReactiveCommand<Unit, Unit>? AddToQueueCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleMenuCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseMenuCommand { get; }

    public ReactiveCommand<Unit, Unit> MenuPlayCommand { get; }

    public ReactiveCommand<Unit, Unit>? MenuPlayNextCommand { get; }

    public ReactiveCommand<Unit, Unit>? MenuToggleFavoriteCommand { get; }

    public ReactiveCommand<Unit, Unit>? MenuAddToQueueCommand { get; }

    public ReactiveCommand<Unit, Unit>? MenuDownloadCommand { get; }

    public ReactiveCommand<Unit, Unit>? MenuShowInfoCommand { get; }

    public void UpdatePlaybackState(string? currentSongId, PlaybackStatus status)
    {
        var isCurrent = currentSongId is not null && currentSongId == Song.Id;
        IsCurrentTrackPlaying = isCurrent && status == PlaybackStatus.Playing;
    }

    private void ToggleMenu()
    {
        var opening = !IsMenuOpen;
        if (opening)
        {
            _onMenuOpening?.Invoke(this);
        }

        IsMenuOpen = opening;
    }

    private void CloseMenu() => IsMenuOpen = false;

    private string BuildSongInfoText()
    {
        var lines = new List<string>
        {
            $"标题：{Title}",
            $"艺术家：{ArtistName}",
            $"专辑：{AlbumName}",
            $"时长：{DurationText}"
        };

        if (Song.Bitrate > 0)
        {
            lines.Add($"码率：{Song.Bitrate} kbps");
        }

        if (Song.Frequency > 0)
        {
            lines.Add($"采样率：{Song.Frequency} Hz");
        }

        if (!string.IsNullOrWhiteSpace(Song.Path))
        {
            lines.Add($"路径：{Song.Path}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private async Task LoadCoverAsync(ICoverCacheService coverCache)
    {
        try
        {
            var bitmap = await coverCache.GetCoverAsync(Song.Id).ConfigureAwait(true);
            Cover = bitmap;
        }
        catch
        {
            Cover = null;
        }
    }
}
