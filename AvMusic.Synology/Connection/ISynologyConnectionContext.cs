using AvMusic.Core.Models;

namespace AvMusic.Synology.Connection;

/// <summary>
/// 当前连接的服务器配置（登录前由 UI 写入）。
/// </summary>
public interface ISynologyConnectionContext
{
    ServerProfile? Server { get; set; }
}
