namespace AvMusic.Synology.Client;

/// <summary>
/// 群晖 Web API 底层 HTTP 客户端。
/// </summary>
public interface ISynologyApiClient
{
    /// <summary>
    /// 当前是否已配置服务器地址。
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// 向指定 CGI 路径发送 POST（application/x-www-form-urlencoded）。
    /// </summary>
    Task<string> PostAsync(
        string cgiPath,
        IReadOnlyDictionary<string, string> parameters,
        string? sessionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 构建带会话的 Web API 完整 URL（用于流/封面等 GET 请求）。
    /// </summary>
    Uri BuildAuthenticatedUri(string cgiPath, IReadOnlyDictionary<string, string> queryParameters, string sessionId);
}
