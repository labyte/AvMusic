using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class ArtistDetailViewModel : LibraryPageBase
{
    public ArtistDetailViewModel(
        Artist artist,
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Artist = artist;
        Player = player;
        SongRows = [];

        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(s => Player.PlaySongAsync(s));
        PlayAllCommand = ReactiveCommand.CreateFromTask(PlayAllAsync);
    }

    public Artist Artist { get; }

    public IMusicPlayerService Player { get; }

    public override string Title => Artist.Name;

    public ObservableCollection<SongRowViewModel> SongRows { get; }

    private List<Song> _loadedSongs = [];

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        return RunSafeAsync(async ct =>
        {
            SongRows.Clear();
            _loadedSongs = [];
            var page = await AudioStation.GetSongsByArtistAsync(Artist.Name, limit: 2000, cancellationToken: ct)
                .ConfigureAwait(true);

            _loadedSongs = page.Items.ToList();
            foreach (var song in _loadedSongs)
            {
                SongRows.Add(new SongRowViewModel(song, s => Player.PlaySongAsync(s)));
            }
        }, token);
    }

    private async Task PlayAllAsync()
    {
        if (_loadedSongs.Count == 0)
        {
            return;
        }

        await Player.PlayQueueAsync(_loadedSongs, 0).ConfigureAwait(false);
    }
}
