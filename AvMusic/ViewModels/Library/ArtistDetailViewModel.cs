using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class ArtistDetailViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;
    private readonly LibraryNavigationService _navigator;
    private bool _isShowingSongs;
    private List<Song> _loadedSongs = [];

    public ArtistDetailViewModel(
        Artist artist,
        IAudioStationService audioStation,
        IAuthService authService,
        LibraryNavigationService navigator,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Artist = artist;
        _navigator = navigator;
        _player = player;
        Albums = [];
        SongRows = [];

        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(s => Player.PlaySongAsync(s));
        PlayAllCommand = ReactiveCommand.CreateFromTask(PlayAllAsync);
        ShowSongsCommand = ReactiveCommand.CreateFromTask(ShowSongsAsync);
        ShowAlbumsCommand = ReactiveCommand.CreateFromTask(LoadAlbumsAsync);
        OpenAlbumCommand = ReactiveCommand.Create<Album>(OpenAlbum);
    }

    public Artist Artist { get; }

    public IMusicPlayerService Player => _player;

    public override string Title => Artist.Name;

    public ObservableCollection<Album> Albums { get; }

    public ObservableCollection<SongRowViewModel> SongRows { get; }

    public bool IsShowingSongs
    {
        get => _isShowingSongs;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isShowingSongs, value);
            this.RaisePropertyChanged(nameof(IsShowingAlbums));
        }
    }

    public bool IsShowingAlbums => !IsShowingSongs;

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowSongsCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowAlbumsCommand { get; }

    public ReactiveCommand<Album, Unit> OpenAlbumCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsShowingSongs = false;
        return LoadAlbumsAsync(cancellationToken);
    }

    private async Task LoadAlbumsAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        await RunSafeAsync(async ct =>
        {
            Albums.Clear();
            IsShowingSongs = false;
            var page = await AudioStation.GetAlbumsByArtistAsync(Artist.Name, cancellationToken: ct)
                .ConfigureAwait(true);

            foreach (var album in page.Items)
            {
                Albums.Add(album);
            }
        }, token);
    }

    private async Task ShowSongsAsync()
    {
        var token = BeginLoadSession();
        await RunSafeAsync(async ct =>
        {
            SongRows.Clear();
            _loadedSongs = [];
            var page = await AudioStation.GetSongsByArtistAsync(Artist.Name, limit: 2000, cancellationToken: ct)
                .ConfigureAwait(true);

            _loadedSongs = page.Items.ToList();
            foreach (var song in _loadedSongs)
            {
                SongRows.Add(new SongRowViewModel(song, s => _player.PlaySongAsync(s)));
            }

            IsShowingSongs = true;
        }, token).ConfigureAwait(false);
    }

    private async Task PlayAllAsync()
    {
        if (_loadedSongs.Count == 0)
        {
            return;
        }

        await _player.PlayQueueAsync(_loadedSongs, 0).ConfigureAwait(false);
    }

    private void OpenAlbum(Album album)
    {
        var detail = new AlbumDetailViewModel(album, AudioStation, AuthService, _player);
        _navigator.NavigateTo(detail);
    }
}
