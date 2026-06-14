using AvMusic.Core.Models;

namespace AvMusic.Core.Session;

/// <summary>
/// 当前 DSM 会话状态。
/// </summary>
public interface ISessionState
{
    bool IsAuthenticated { get; }

    string? SessionId { get; }

    ServerProfile? Server { get; }

    event EventHandler? SessionChanged;

    void SetSession(ServerProfile server, string sessionId);

    void Clear();
}
