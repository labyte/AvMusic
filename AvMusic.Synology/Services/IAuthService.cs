using AvMusic.Core.Models;

namespace AvMusic.Synology.Services;

public interface IAuthService
{
    Task LoginAsync(
        ServerProfile server,
        string account,
        string password,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
