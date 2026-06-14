using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class GenresLibraryViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;

    public GenresLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        LibraryNavigationService navigator,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Navigator = navigator;
        _player = player;
        Genres = [];

        OpenGenreCommand = ReactiveCommand.Create<Genre>(OpenGenre);
    }

    public LibraryNavigationService Navigator { get; }

    public override string Title => "类型";

    public ObservableCollection<Genre> Genres { get; }

    public ReactiveCommand<Genre, Unit> OpenGenreCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        return RunSafeAsync(async ct =>
        {
            Genres.Clear();
            var list = await AudioStation.GetGenresAsync(ct).ConfigureAwait(true);

            foreach (var genre in list.Where(g => !string.IsNullOrWhiteSpace(g.Name)))
            {
                Genres.Add(genre);
            }
        }, token);
    }

    private void OpenGenre(Genre genre)
    {
        var detail = new GenreAlbumsViewModel(genre, AudioStation, AuthService, Navigator, _player);
        Navigator.NavigateTo(detail);
    }
}
