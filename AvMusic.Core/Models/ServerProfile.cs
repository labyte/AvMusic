namespace AvMusic.Core.Models;

/// <summary>
/// 群晖 NAS 连接配置。
/// </summary>
public sealed class ServerProfile
{
    public required string Host { get; init; }

    public int Port { get; init; } = 5001;

    public bool UseHttps { get; init; } = true;

    /// <summary>
    /// 是否信任自签名证书（开发/内网场景）。
    /// </summary>
    public bool TrustAllCertificates { get; init; }

    public string BaseUrl =>
        $"{(UseHttps ? "https" : "http")}://{Host.TrimEnd('/')}:{Port}";

    public string WebApiBaseUrl => $"{BaseUrl}/webapi";
}
