namespace AvMusic.Models;

/// <summary>
/// 本地持久化的服务器与账号配置。
/// </summary>
public sealed class ServerSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 5001;

    public bool UseHttps { get; set; } = true;

    public bool TrustAllCertificates { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>受保护存储的密码（RememberCredentials 为 true 时）。</summary>
    public string? ProtectedPassword { get; set; }

    public bool RememberCredentials { get; set; }
}
