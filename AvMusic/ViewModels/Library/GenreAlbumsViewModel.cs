using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class GenreAlbumsViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;
    private readonly LibraryNavigationService _navigator;

    public GenreAlbumsViewModel(
        Genre genre,
        IAudioStationService audioStation,
        IAuthService authService,
        LibraryNavigationService navigator,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Genre = genre;
        _navigator = navigator;
        _player = player;
        Albums = [];

        OpenAlbumCommand = ReactiveCommand.Create<Album>(OpenAlbum);
    }

    public Genre Genre { get; }

    public override string Title => Genre.Name;

    public ObservableCollection<Album> Albums { get; }

    public ReactiveCommand<Album, Unit> OpenAlbumCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        return RunSafeAsync(async ct =>
        {
            Albums.Clear();
            var page = await AudioStation.GetAlbumsByGenreAsync(Genre.Name, cancellationToken: ct)
                .ConfigureAwait(true);

            foreach (var album in page.Items)
            {
                Albums.Add(album);
            }
        }, token);
    }

    private void OpenAlbum(Album album)
    {
        var detail = new AlbumDetailViewModel(album, AudioStation, AuthService, _player);
        _navigator.NavigateTo(detail);
    }
}
