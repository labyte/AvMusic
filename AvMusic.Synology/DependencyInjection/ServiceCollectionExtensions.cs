using AvMusic.Core.Session;
using AvMusic.Synology.Client;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Options;
using AvMusic.Synology.Services;
using AvMusic.Synology.Streaming;
using Microsoft.Extensions.DependencyInjection;

namespace AvMusic.Synology.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册群晖 API 相关服务。
    /// </summary>
    public static IServiceCollection AddSynologyServices(this IServiceCollection services)
    {
        services.AddOptions<SynologyClientOptions>();
        services.AddSingleton<ISynologyConnectionContext, SynologyConnectionContext>();
        services.AddSingleton<ISessionState, SessionState>();
        services.AddSingleton<ISynologyApiClient, SynologyApiClient>();
        services.AddSingleton<ISynologyMediaHttpClient, SynologyMediaHttpClient>();
        services.AddSingleton<IApiInfoService, ApiInfoService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAudioStationService, AudioStationService>();
        services.AddSingleton<StreamUrlBuilder>();
        services.AddSingleton<SynologyStreamDownloader>();
        return services;
    }
}
