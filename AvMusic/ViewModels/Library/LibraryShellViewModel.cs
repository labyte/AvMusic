using System.Reactive;
using AvMusic.Services;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public class LibraryShellViewModel : ViewModelBase
{
    private readonly Stack<ViewModelBase> _backStack = new();
    private ViewModelBase _currentPage;
    private bool _canGoBack;

    public LibraryShellViewModel(
        LibraryNavigationService navigation,
        SongsLibraryViewModel songs,
        AlbumsLibraryViewModel albums,
        ArtistsLibraryViewModel artists,
        SearchLibraryViewModel search,
        FolderBrowserViewModel folders,
        GenresLibraryViewModel genres)
    {
        navigation.Bind(NavigateTo);

        Songs = songs;
        Albums = albums;
        Artists = artists;
        Search = search;
        Folders = folders;
        Genres = genres;

        _currentPage = Songs;

        NavigateCommand = ReactiveCommand.Create<string>(Navigate);
        GoBackCommand = ReactiveCommand.Create(GoBack, this.WhenAnyValue(x => x.CanGoBack));

        _ = Songs.LoadAsync();
    }

    public SongsLibraryViewModel Songs { get; }

    public AlbumsLibraryViewModel Albums { get; }

    public ArtistsLibraryViewModel Artists { get; }

    public SearchLibraryViewModel Search { get; }

    public FolderBrowserViewModel Folders { get; }

    public GenresLibraryViewModel Genres { get; }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            this.RaisePropertyChanged(nameof(CurrentPageTitle));
        }
    }

    public string CurrentPageTitle => (CurrentPage as ILibraryPage)?.Title ?? "音乐库";

    public bool CanGoBack
    {
        get => _canGoBack;
        private set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }

    public ReactiveCommand<string, Unit> NavigateCommand { get; }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public void NavigateTo(ViewModelBase page, bool pushBack = true)
    {
        if (pushBack && CurrentPage is not null)
        {
            _backStack.Push(CurrentPage);
        }

        CurrentPage = page;
        CanGoBack = _backStack.Count > 0;

        if (page is ILibraryPage loadable)
        {
            _ = loadable.LoadAsync();
        }
    }

    private void Navigate(string key)
    {
        _backStack.Clear();
        CanGoBack = false;

        CurrentPage = key switch
        {
            "songs" => Songs,
            "albums" => Albums,
            "artists" => Artists,
            "search" => Search,
            "folders" => Folders,
            "genres" => Genres,
            _ => Songs
        };

        if (CurrentPage is ILibraryPage page)
        {
            _ = page.LoadAsync();
        }
    }

    private void GoBack()
    {
        if (_backStack.Count == 0)
        {
            return;
        }

        CurrentPage = _backStack.Pop();
        CanGoBack = _backStack.Count > 0;

        if (CurrentPage is ILibraryPage page)
        {
            _ = page.LoadAsync();
        }
    }
}
