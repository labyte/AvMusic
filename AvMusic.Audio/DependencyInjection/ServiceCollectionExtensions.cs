using AvMusic.Audio.LibVlc;
using AvMusic.Core.Playback;
using Microsoft.Extensions.DependencyInjection;
namespace AvMusic.Audio.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册音频播放服务。Desktop 使用 LibVLC，其他平台回退 Stub。
    /// </summary>
    public static IServiceCollection AddAudioServices(
        this IServiceCollection services,
        bool useLibVlc = true)
    {
        services.AddSingleton<IPlaybackQueue, PlaybackQueue>();

        if (useLibVlc && IsDesktopPlatform())
        {
            services.AddSingleton<LibVlcHost>();
            services.AddSingleton<IPlaybackEngine, LibVlcPlaybackEngine>();
        }
        else
        {
            services.AddSingleton<IPlaybackEngine, StubPlaybackEngine>();
        }

        return services;
    }

    private static bool IsDesktopPlatform() =>
        OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
}
