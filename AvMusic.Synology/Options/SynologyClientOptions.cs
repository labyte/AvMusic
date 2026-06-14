using AvMusic.Core.Models;

namespace AvMusic.Synology.Options;

/// <summary>
/// 群晖 API 客户端运行时选项。
/// </summary>
public sealed class SynologyClientOptions
{
    public ServerProfile? Server { get; set; }

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
