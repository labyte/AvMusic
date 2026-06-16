using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace AvMusic.ViewModels;

/// <summary>
/// 播放列表抽屉中的单行。
/// </summary>
public sealed class QueueRowViewModel : ViewModelBase
{
    private readonly PlayerViewModel _player;
    private readonly Action<QueueRowViewModel>? _onMenuOpening;
    private readonly Action<QueueRowViewModel>? _onMenuClosed;
    private int _rating;
    private bool _isCurrent;
    private bool _isCurrentPlaying;
    private bool _isMenuOpen;
    private Bitmap? _cover;

    public QueueRowViewModel(
        PlayerViewModel player,
        Song song,
        int index,
        bool isCurrent,
        bool isCurrentPlaying,
        Action<QueueRowViewModel>? onMenuOpening,
        Action<QueueRowViewModel>? onMenuClosed)
    {
        _player = player;
        Song = song;
        Index = index;
        _rating = song.Rating;
        _isCurrent = isCurrent;
        _isCurrentPlaying = isCurrentPlaying;
        _onMenuOpening = onMenuOpening;
        _onMenuClosed = onMenuClosed;

        PlayCommand = ReactiveCommand.CreateFromTask(PlayAsync);
        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        ToggleMenuCommand = ReactiveCommand.Create(ToggleMenu);
        MenuPlayCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CloseMenu();
            await PlayAsync().ConfigureAwait(true);
        });
        MenuFavoriteCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CloseMenu();
            await ToggleFavoriteAsync().ConfigureAwait(true);
        });
        MenuRemoveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CloseMenu();
            await _player.RemoveQueueItemAsync(Index).ConfigureAwait(true);
        });

        _ = LoadCoverAsync();
    }

    public Song Song { get; }

    public int Index { get; private set; }

    public string Title => Song.Title;

    public string ArtistName => string.IsNullOrWhiteSpace(Song.Artist) ? "未知艺术家" : Song.Artist;

    public string DurationText => TimeSpan.FromSeconds(Song.DurationSeconds).ToString(@"m\:ss");

    public bool IsFavorite => _rating >= 5;

    public bool IsCurrent
    {
        get => _isCurrent;
        private set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
    }

    /// <summary>当前行是否为正在播放的歌曲。</summary>
    public bool IsCurrentPlaying
    {
        get => _isCurrentPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isCurrentPlaying, value);
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

    public Bitmap? Cover
    {
        get => _cover;
        private set => this.RaiseAndSetIfChanged(ref _cover, value);
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleMenuCommand { get; }

    public ReactiveCommand<Unit, Unit> MenuPlayCommand { get; }

    public ReactiveCommand<Unit, Unit> MenuFavoriteCommand { get; }

    public ReactiveCommand<Unit, Unit> MenuRemoveCommand { get; }

    public void UpdateIndex(int index, bool isCurrent, bool isCurrentPlaying)
    {
        Index = index;
        IsCurrent = isCurrent;
        IsCurrentPlaying = isCurrentPlaying;
        this.RaisePropertyChanged(nameof(Index));
    }

    public void UpdatePlaybackState(bool isCurrent, bool isCurrentPlaying)
    {
        IsCurrent = isCurrent;
        IsCurrentPlaying = isCurrentPlaying;
    }

    public void UpdateRating(int rating)
    {
        _rating = rating;
        this.RaisePropertyChanged(nameof(IsFavorite));
    }

    private async Task PlayAsync() => await _player.PlayQueueItemAsync(Index).ConfigureAwait(true);

    private async Task ToggleFavoriteAsync()
    {
        var newRating = await _player.ToggleSongFavoriteAsync(Song, IsFavorite).ConfigureAwait(true);
        _rating = newRating;
        this.RaisePropertyChanged(nameof(IsFavorite));
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

    private async Task LoadCoverAsync()
    {
        try
        {
            Cover = await _player.GetCoverAsync(Song.Id).ConfigureAwait(true);
        }
        catch
        {
            Cover = null;
        }
    }
}
