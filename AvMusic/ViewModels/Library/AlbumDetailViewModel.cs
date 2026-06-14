using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class AlbumDetailViewModel : LibraryPageBase
{
    public AlbumDetailViewModel(
        Album album,
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Album = album;
        Player = player;
        SongRows = [];

        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(PlaySongAsync);
        PlayAllCommand = ReactiveCommand.CreateFromTask(PlayAllAsync);
    }

    public Album Album { get; }

    public IMusicPlayerService Player { get; }

    public override string Title => Album.Name;

    public string Header => $"{Album.DisplayArtist ?? Album.AlbumArtist} · {Album.Name}";

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
            var page = await AudioStation.GetSongsByAlbumAsync(
                    Album.Name,
                    Album.AlbumArtist ?? Album.DisplayArtist ?? string.Empty,
                    cancellationToken: ct)
                .ConfigureAwait(true);

            _loadedSongs = page.Items.ToList();
            foreach (var song in _loadedSongs)
            {
                SongRows.Add(new SongRowViewModel(song, PlaySongAsync));
            }
        }, token);
    }

    private async Task PlaySongAsync(Song song) =>
        await Player.PlaySongAsync(song).ConfigureAwait(false);

    private async Task PlayAllAsync()
    {
        if (_loadedSongs.Count == 0)
        {
            return;
        }

        await Player.PlayQueueAsync(_loadedSongs, 0).ConfigureAwait(false);
    }
}
