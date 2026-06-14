namespace AvMusic.Core.Models;

/// <summary>
/// 规范化用户输入的服务器地址（避免 https://https:// 等错误 URL）。
/// </summary>
public static class ServerProfileParser
{
    public static ServerProfile Create(
        string host,
        int port,
        bool useHttps,
        bool trustAllCertificates)
    {
        var (normalizedHost, normalizedPort, normalizedHttps) = Parse(host, port, useHttps);

        return new ServerProfile
        {
            Host = normalizedHost,
            Port = normalizedPort,
            UseHttps = normalizedHttps,
            TrustAllCertificates = trustAllCertificates
        };
    }

    public static (string Host, int Port, bool UseHttps) Parse(string host, int port, bool useHttps)
    {
        var value = host.Trim();
        var https = useHttps;

        if (value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = value[8..];
            https = true;
        }
        else if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            value = value[7..];
            https = false;
        }

        value = value.TrimEnd('/');

        // 用户输入 192.168.1.1:5001 时拆分端口
        var colon = value.LastIndexOf(':');
        if (colon > 0 && int.TryParse(value.AsSpan(colon + 1), out var embeddedPort))
        {
            value = value[..colon];
            port = embeddedPort;
        }

        if (port <= 0)
        {
            port = https ? 5001 : 5000;
        }

        return (value, port, https);
    }
}
