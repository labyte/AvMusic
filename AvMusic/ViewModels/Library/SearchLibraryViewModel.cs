using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class SearchLibraryViewModel : LibraryPageBase
{
    private string _keyword = string.Empty;

    public SearchLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Player = player;
        SongRows = [];
        Albums = [];
        Artists = [];

        var canSearch = this.WhenAnyValue(x => x.Keyword)
            .Select(k => !string.IsNullOrWhiteSpace(k));

        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync, canSearch);
        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(s => Player.PlaySongAsync(s));
    }

    public IMusicPlayerService Player { get; }

    public override string Title => "搜索";

    public string Keyword
    {
        get => _keyword;
        set => this.RaiseAndSetIfChanged(ref _keyword, value);
    }

    public ObservableCollection<SongRowViewModel> SongRows { get; }

    public ObservableCollection<Album> Albums { get; }

    public ObservableCollection<Artist> Artists { get; }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private Task SearchAsync()
    {
        var token = BeginLoadSession();
        return RunSafeAsync(async ct =>
        {
            SongRows.Clear();
            Albums.Clear();
            Artists.Clear();

            var result = await AudioStation.SearchAsync(Keyword.Trim(), limit: 100, cancellationToken: ct)
                .ConfigureAwait(true);

            foreach (var song in result.Songs)
            {
                SongRows.Add(new SongRowViewModel(song, s => Player.PlaySongAsync(s)));
            }

            foreach (var album in result.Albums)
            {
                Albums.Add(album);
            }

            foreach (var artist in result.Artists.Where(a => !string.IsNullOrWhiteSpace(a.Name)))
            {
                Artists.Add(artist);
            }
        }, token);
    }
}
