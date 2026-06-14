using AvMusic.ViewModels;
using AvMusic.ViewModels.Library;

namespace AvMusic.Services;

/// <summary>
/// 解耦音乐库页面导航，避免 DI 循环依赖。
/// </summary>
public sealed class LibraryNavigationService : ILibraryNavigator
{
    private Action<ViewModelBase, bool>? _navigate;

    internal void Bind(Action<ViewModelBase, bool> navigate) => _navigate = navigate;

    public void NavigateTo(ViewModelBase page, bool pushBack = true) =>
        _navigate?.Invoke(page, pushBack);
}
