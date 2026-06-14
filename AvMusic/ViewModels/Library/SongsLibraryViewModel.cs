using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class SongsLibraryViewModel : LibraryPageBase
{
    private const int PageSize = 100;
    private int _offset;
    private bool _hasMore = true;

    public SongsLibraryViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        Player = player;
        SongRows = [];

        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(PlaySongAsync);
        LoadMoreCommand = ReactiveCommand.CreateFromTask(LoadMoreAsync, this.WhenAnyValue(
            x => x.IsLoading,
            x => x.HasMore,
            (loading, more) => !loading && more));
    }

    public IMusicPlayerService Player { get; }

    public override string Title => "全部歌曲";

    public ObservableCollection<SongRowViewModel> SongRows { get; }

    public bool HasMore
    {
        get => _hasMore;
        private set => this.RaiseAndSetIfChanged(ref _hasMore, value);
    }

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var token = BeginLoadSession(cancellationToken);
        _offset = 0;
        HasMore = true;
        SongRows.Clear();
        return LoadMoreAsync(token);
    }

    private Task LoadMoreAsync(CancellationToken cancellationToken = default) =>
        RunSafeAsync(async ct =>
        {
            var page = await AudioStation.GetSongsAsync(_offset, PageSize, cancellationToken: ct)
                .ConfigureAwait(true);

            ct.ThrowIfCancellationRequested();

            foreach (var song in page.Items)
            {
                SongRows.Add(new SongRowViewModel(song, PlaySongAsync));
            }

            _offset += page.Items.Count;
            HasMore = page.HasMore;
        }, cancellationToken);

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
}
