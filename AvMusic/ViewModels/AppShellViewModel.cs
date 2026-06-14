using AvMusic.Core.Session;
using ReactiveUI;

namespace AvMusic.ViewModels;

/// <summary>
/// 应用外壳：根据登录状态切换登录页与主页。
/// </summary>
public class AppShellViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;

    public AppShellViewModel(
        ISessionState sessionState,
        LoginViewModel loginViewModel,
        MainViewModel mainViewModel)
    {
        SessionState = sessionState;
        LoginViewModel = loginViewModel;
        MainViewModel = mainViewModel;
        _currentViewModel = sessionState.IsAuthenticated ? mainViewModel : loginViewModel;

        SessionState.SessionChanged += OnSessionChanged;
    }

    public ISessionState SessionState { get; }

    public LoginViewModel LoginViewModel { get; }

    public MainViewModel MainViewModel { get; }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    private void OnSessionChanged(object? sender, EventArgs e) =>
        CurrentViewModel = SessionState.IsAuthenticated ? MainViewModel : LoginViewModel;
}
