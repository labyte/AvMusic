using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class AlbumsLibraryViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;

    public AlbumsLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        LibraryNavigationService navigator,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Navigator = navigator;
        _player = player;
        Albums = [];

        OpenAlbumCommand = ReactiveCommand.Create<Album>(OpenAlbum);
    }

    public LibraryNavigationService Navigator { get; }

    public override string Title => "专辑";

    public ObservableCollection<Album> Albums { get; }

    public ReactiveCommand<Album, Unit> OpenAlbumCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        return RunSafeAsync(async ct =>
        {
            Albums.Clear();
            var page = await AudioStation.GetAlbumsAsync(limit: 2000, cancellationToken: ct)
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
        Navigator.NavigateTo(detail);
    }
}
