using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class ArtistsLibraryViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;

    public ArtistsLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        LibraryNavigationService navigator,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Navigator = navigator;
        _player = player;
        Artists = [];

        OpenArtistCommand = ReactiveCommand.Create<Artist>(OpenArtist);
    }

    public LibraryNavigationService Navigator { get; }

    public override string Title => "艺术家";

    public ObservableCollection<Artist> Artists { get; }

    public ReactiveCommand<Artist, Unit> OpenArtistCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        return RunSafeAsync(async ct =>
        {
            Artists.Clear();
            var page = await AudioStation.GetArtistsAsync(limit: 2000, cancellationToken: ct)
                .ConfigureAwait(true);

            foreach (var artist in page.Items.Where(a => !string.IsNullOrWhiteSpace(a.Name)))
            {
                Artists.Add(artist);
            }
        }, token);
    }

    private void OpenArtist(Artist artist)
    {
        var detail = new ArtistDetailViewModel(artist, AudioStation, AuthService, Navigator, _player);
        Navigator.NavigateTo(detail);
    }
}
