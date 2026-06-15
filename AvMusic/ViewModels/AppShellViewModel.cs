using AvMusic.Core.Session;
using Avalonia.Threading;
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

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        // 登录/登出可能在后台线程完成，必须在 UI 线程切换页面并创建 View
        var next = (ViewModelBase)(SessionState.IsAuthenticated ? MainViewModel : LoginViewModel);
        if (Dispatcher.UIThread.CheckAccess())
        {
            CurrentViewModel = next;
        }
        else
        {
            Dispatcher.UIThread.Post(() => CurrentViewModel = next);
        }
    }
}
