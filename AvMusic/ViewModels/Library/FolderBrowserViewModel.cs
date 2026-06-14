using System.Collections.ObjectModel;
using System.Reactive;
using AvMusic.Core.Models;
using AvMusic.Services;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public sealed class FolderBrowserViewModel : LibraryPageBase
{
    private readonly IMusicPlayerService _player;
    private string? _currentFolderId;

    public FolderBrowserViewModel(
        IAudioStationService audioStation,
        IAuthService authService,
        IMusicPlayerService player)
        : base(audioStation, authService)
    {
        _player = player;
        Subfolders = [];
        Songs = [];

        OpenFolderCommand = ReactiveCommand.Create<MusicFolder>(OpenFolder);
        PlaySongCommand = ReactiveCommand.CreateFromTask<Song>(s => _player.PlaySongAsync(s));
    }

    public override string Title => "文件夹";

    public string CurrentPath { get; private set; } = "根目录";

    public ObservableCollection<MusicFolder> Subfolders { get; }

    public ObservableCollection<Song> Songs { get; }

    public ReactiveCommand<MusicFolder, Unit> OpenFolderCommand { get; }

    public ReactiveCommand<Song, Unit> PlaySongCommand { get; }

    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _currentFolderId = null;
        CurrentPath = "根目录";
        this.RaisePropertyChanged(nameof(CurrentPath));
        var token = BeginLoadSession(cancellationToken);
        return BrowseAsync(token);
    }

    private void OpenFolder(MusicFolder folder)
    {
        _currentFolderId = folder.Id;
        CurrentPath = folder.Title;
        this.RaisePropertyChanged(nameof(CurrentPath));
        var token = BeginLoadSession();
        _ = BrowseAsync(token);
    }

    private Task BrowseAsync(CancellationToken cancellationToken = default) =>
        RunSafeAsync(async ct =>
        {
            Subfolders.Clear();
            Songs.Clear();

            var result = await AudioStation.BrowseFolderAsync(_currentFolderId, ct).ConfigureAwait(true);

            foreach (var folder in result.Subfolders)
            {
                Subfolders.Add(folder);
            }

            foreach (var song in result.Songs)
            {
                Songs.Add(song);
            }
        }, cancellationToken);
}
