using Avalonia.Media.Imaging;

namespace AvMusic.Services;

public interface ICoverCacheService
{
    Task<Bitmap?> GetCoverAsync(string songId, CancellationToken cancellationToken = default);
}
