using AvMusic.Audio.DependencyInjection;
using AvMusic.Core.Threading;
using AvMusic.Services;
using AvMusic.Synology.DependencyInjection;
using AvMusic.ViewModels;
using AvMusic.ViewModels.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AvMusic;

/// <summary>
/// 应用级依赖注入配置。
/// </summary>
public static class AppServices
{
    private static IServiceProvider? _provider;

    public static IServiceProvider Provider =>
        _provider ?? throw new InvalidOperationException("应用服务尚未初始化，请先调用 Configure。");

    public static void Configure()
    {
        if (_provider is not null)
        {
            return;
        }

        var services = new ServiceCollection();

        services.AddLogging(static builder => builder.SetMinimumLevel(LogLevel.Debug));

        services.AddSynologyServices();
        services.AddSingleton<IMainThreadDispatcher, AvaloniaMainThreadDispatcher>();
        services.AddAudioServices(useLibVlc: IsDesktopPlatform());
        services.AddSingleton<IMusicPlayerService, MusicPlayerService>();
        services.AddSingleton<IServerSettingsStore, JsonServerSettingsStore>();
        services.AddSingleton<ICoverCacheService, CoverCacheService>();
        services.AddSingleton<LibraryNavigationService>();
        services.AddSingleton<ILibraryNavigator>(sp => sp.GetRequiredService<LibraryNavigationService>());

        services.AddSingleton<SongsLibraryViewModel>();
        services.AddSingleton<AlbumsLibraryViewModel>();
        services.AddSingleton<ArtistsLibraryViewModel>();
        services.AddSingleton<SearchLibraryViewModel>();
        services.AddSingleton<FolderBrowserViewModel>();
        services.AddSingleton<GenresLibraryViewModel>();
        services.AddSingleton<LibraryShellViewModel>();
        services.AddSingleton<PlayerViewModel>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AppShellViewModel>();

        _provider = services.BuildServiceProvider();
    }

    private static bool IsDesktopPlatform() =>
        OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    public static T GetRequiredService<T>() where T : notnull =>
        Provider.GetRequiredService<T>();
}
