namespace AvMusic.Synology.Client;

/// <summary>
/// 用于封面、流媒体等二进制资源的 HttpClient（与 API 客户端共用证书策略）。
/// </summary>
public interface ISynologyMediaHttpClient
{
    /// <summary>封面等较小资源。</summary>
    HttpClient Client { get; }

    /// <summary>整曲下载/拉流（较长超时）。</summary>
    HttpClient StreamClient { get; }

    /// <summary>
    /// 登录/登出或切换服务器后调用，重建 HttpClient。
    /// </summary>
    void Reset();
}
