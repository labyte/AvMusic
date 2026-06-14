using AvMusic.Core.Models;

namespace AvMusic.Core.Session;

public sealed class SessionState : ISessionState
{
    public bool IsAuthenticated => !string.IsNullOrEmpty(SessionId);

    public string? SessionId { get; private set; }

    public ServerProfile? Server { get; private set; }

    public event EventHandler? SessionChanged;

    public void SetSession(ServerProfile server, string sessionId)
    {
        Server = server;
        SessionId = sessionId;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Server = null;
        SessionId = null;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
