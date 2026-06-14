using AvMusic.Models;

namespace AvMusic.Services;

public interface IServerSettingsStore
{
    Task<ServerSettings?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ServerSettings settings, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
