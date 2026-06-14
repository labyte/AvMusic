using System.Reactive;
using System.Reactive.Linq;
using AvMusic.Core.Session;
using AvMusic.Services;
using AvMusic.Synology.Services;
using AvMusic.ViewModels.Library;
using ReactiveUI;

namespace AvMusic.ViewModels;

public class MainViewModel : ViewModelBase
{
    private bool _isLoggingOut;

    public MainViewModel(
        ISessionState sessionState,
        IAuthService authService,
        IMusicPlayerService player,
        LibraryShellViewModel library,
        PlayerViewModel playerUi)
    {
        SessionState = sessionState;
        AuthService = authService;
        Player = player;
        Library = library;
        PlayerUi = playerUi;

        LogoutCommand = ReactiveCommand.CreateFromTask(
            LogoutAsync,
            this.WhenAnyValue(x => x.IsLoggingOut, busy => !busy));
    }

    public ISessionState SessionState { get; }

    public IAuthService AuthService { get; }

    public IMusicPlayerService Player { get; }

    public LibraryShellViewModel Library { get; }

    public PlayerViewModel PlayerUi { get; }

    public bool IsLoggingOut
    {
        get => _isLoggingOut;
        private set => this.RaiseAndSetIfChanged(ref _isLoggingOut, value);
    }

    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    private async Task LogoutAsync()
    {
        IsLoggingOut = true;
        try
        {
            await Player.StopAsync().ConfigureAwait(false);
            Player.Queue.Clear();
            await AuthService.LogoutAsync().ConfigureAwait(false);
        }
        finally
        {
            IsLoggingOut = false;
        }
    }
}
