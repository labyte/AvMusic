using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Core.Playback;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class SongsLibraryViewModel : LibraryPageBase
{
    private const int PageSize = 100;
    private int _offset;
    private bool _hasMore = true;
    private int _totalCount;
    private SongRowViewModel? _infoRow;
    private SongRowViewModel? _openMenuRow;

    public SongsLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player,
        ICoverCacheService coverCache,
        ILibraryNavigator navigator)
        : base(audioStation, authService)
    {
        Player = player;
        CoverCache = coverCache;
        Navigator = navigator;
        SongRows = [];

        Player.Engine.StateChanged += (_, _) => RefreshPlaybackState();
        Player.Queue.QueueChanged += (_, _) => RefreshPlaybackState();

        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(PlaySongAsync);
        LoadMoreCommand = ReactiveCommand.CreateFromTask(LoadMoreAsync, this.WhenAnyValue(
            x => x.IsLoading,
            x => x.HasMore,
            (loading, more) => !loading && more));

        var hasSongs = this.WhenAnyValue(x => x.LoadedCount, count => count > 0);
        PlayAllCommand = ReactiveCommand.CreateFromTask(PlayAllAsync, hasSongs);
        AddAllToQueueCommand = ReactiveCommand.CreateFromTask(AddAllToQueueAsync, hasSongs);
        CloseInfoCommand = ReactiveCommand.Create(CloseInfo);
    }

    public IMusicPlayerService Player { get; }

    public ICoverCacheService CoverCache { get; }

    public ILibraryNavigator Navigator { get; }

    public override string Title => "全部歌曲";

    public ObservableCollection<SongRowViewModel> SongRows { get; }

    public SongRowViewModel? InfoRow
    {
        get => _infoRow;
        private set => this.RaiseAndSetIfChanged(ref _infoRow, value);
    }

    public int LoadedCount => SongRows.Count;

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            this.RaiseAndSetIfChanged(ref _totalCount, value);
            this.RaisePropertyChanged(nameof(SongCountSummary));
        }
    }

    public string SongCountSummary => TotalCount > 0
        ? $"共 {LoadedCount}/{TotalCount} 首歌曲"
        : LoadedCount > 0 ? $"共 {LoadedCount} 首歌曲" : "正在加载…";

    public bool HasMore
    {
        get => _hasMore;
        private set => this.RaiseAndSetIfChanged(ref _hasMore, value);
    }

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }

    public ReactiveCommand<Unit, Unit> PlayAllCommand { get; }

    public ReactiveCommand<Unit, Unit> AddAllToQueueCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseInfoCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        _offset = 0;
        HasMore = true;
        TotalCount = 0;
        SongRows.Clear();
        this.RaisePropertyChanged(nameof(LoadedCount));
        this.RaisePropertyChanged(nameof(SongCountSummary));
        return LoadMoreAsync(token);
    }

    public void RequestLoadMore()
    {
        if (IsLoading || !HasMore)
        {
            return;
        }

        _ = LoadMoreAsync();
    }

    private Task LoadMoreAsync(CancellationToken cancellationToken = default) =>
        RunSafeAsync(async ct =>
        {
            var page = await AudioStation.GetSongsAsync(_offset, PageSize, cancellationToken: ct)
                .ConfigureAwait(true);

            ct.ThrowIfCancellationRequested();

            TotalCount = page.Total;

            foreach (var song in page.Items)
            {
                SongRows.Add(CreateRow(song, SongRows.Count + 1));
            }

            _offset += page.Items.Count;
            HasMore = page.HasMore;
            this.RaisePropertyChanged(nameof(LoadedCount));
            this.RaisePropertyChanged(nameof(SongCountSummary));
            RefreshPlaybackState();
        }, cancellationToken);

    private SongRowViewModel CreateRow(Song song, int index) =>
        new(
            song,
            index,
            PlaySongAsync,
            ToggleFavoriteAsync,
            OpenArtistAsync,
            OpenAlbumAsync,
            AddSongToQueueAsync,
            PlayNextAsync,
            TogglePlayPauseAsync,
            DownloadSongAsync,
            ShowSongInfo,
            OnMenuOpening,
            OnMenuClosed,
            CoverCache,
            Player);

    private async Task PlayAllAsync()
    {
        var songs = SongRows.Select(row => row.Song).ToList();
        if (songs.Count == 0)
        {
            return;
        }

        await Player.PlayQueueAsync(songs, 0).ConfigureAwait(false);
    }

    private Task AddAllToQueueAsync()
    {
        var songs = SongRows.Select(row => row.Song).ToList();
        if (songs.Count == 0)
        {
            return Task.CompletedTask;
        }

        Player.Queue.AppendSongs(songs);
        return Task.CompletedTask;
    }

    private Task AddSongToQueueAsync(Song song)
    {
        Player.Queue.AppendSongs([song]);
        return Task.CompletedTask;
    }

    private Task PlayNextAsync(Song song)
    {
        Player.Queue.InsertNext(song);
        return Task.CompletedTask;
    }

    private async Task TogglePlayPauseAsync(Song song)
    {
        if (Player.Queue.Current?.Id == song.Id
            && Player.Engine.State.Status == PlaybackStatus.Playing)
        {
            await Player.PauseAsync().ConfigureAwait(true);
            return;
        }

        if (Player.Queue.Current?.Id == song.Id)
        {
            await Player.PlayAsync().ConfigureAwait(true);
            return;
        }

        await PlaySongAsync(song).ConfigureAwait(true);
    }

    private Task OpenArtistAsync(Song song)
    {
        if (string.IsNullOrWhiteSpace(song.Artist))
        {
            return Task.CompletedTask;
        }

        var detail = new ArtistDetailViewModel(
            new Artist { Name = song.Artist },
            AudioStation,
            AuthService,
            Player);
        Navigator.NavigateTo(detail);
        return Task.CompletedTask;
    }

    private Task OpenAlbumAsync(Song song)
    {
        if (string.IsNullOrWhiteSpace(song.Album))
        {
            return Task.CompletedTask;
        }

        var detail = new AlbumDetailViewModel(
            new Album
            {
                Name = song.Album,
                AlbumArtist = song.AlbumArtist ?? song.Artist,
                DisplayArtist = song.Artist
            },
            AudioStation,
            AuthService,
            Player);
        Navigator.NavigateTo(detail);
        return Task.CompletedTask;
    }

    private Task DownloadSongAsync(Song song)
    {
        ErrorMessage = $"下载功能开发中：{song.Title}";
        return Task.CompletedTask;
    }

    private void ShowSongInfo(SongRowViewModel row)
    {
        CloseOpenMenu();

        if (InfoRow is not null)
        {
            InfoRow.IsInfoOpen = false;
        }

        InfoRow = row;
        row.IsInfoOpen = true;
    }

    private void OnMenuOpening(SongRowViewModel row)
    {
        if (_openMenuRow is not null && _openMenuRow != row)
        {
            _openMenuRow.IsMenuOpen = false;
        }

        _openMenuRow = row;
    }

    private void OnMenuClosed(SongRowViewModel row)
    {
        if (_openMenuRow == row)
        {
            _openMenuRow = null;
        }
    }

    private void CloseOpenMenu()
    {
        if (_openMenuRow is not null)
        {
            _openMenuRow.IsMenuOpen = false;
        }
    }

    private void CloseInfo()
    {
        if (InfoRow is not null)
        {
            InfoRow.IsInfoOpen = false;
            InfoRow = null;
        }
    }

    private async Task ToggleFavoriteAsync(Song song, int rating)
    {
        await AudioStation.SetSongRatingAsync(song.Id, rating).ConfigureAwait(true);
    }

    private async Task PlaySongAsync(Song song)
    {
        ErrorMessage = null;
        try
        {
            await Player.PlaySongAsync(song).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // ReactiveCommand 或切换歌曲时取消，不提示错误
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "网络请求失败。HTTPS 自签名证书请勾选登录页的「信任证书」。";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void RefreshPlaybackState()
    {
        var currentId = Player.Queue.Current?.Id;
        var status = Player.Engine.State.Status;

        foreach (var row in SongRows)
        {
            row.UpdatePlaybackState(currentId, status);
        }
    }
}
