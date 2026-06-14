using System.Collections.Concurrent;
using System.Net.Http;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvMusic.Core.Session;
using AvMusic.Synology.Client;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Streaming;
using Microsoft.Extensions.Logging;

namespace AvMusic.Services;

public sealed class CoverCacheService : ICoverCacheService
{
    private readonly ISynologyMediaHttpClient _http;
    private readonly ISessionState _session;
    private readonly ISynologyConnectionContext _connection;
    private readonly StreamUrlBuilder _urlBuilder;
    private readonly ILogger<CoverCacheService> _logger;
    private readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    public CoverCacheService(
        ISynologyMediaHttpClient http,
        ISessionState session,
        ISynologyConnectionContext connection,
        StreamUrlBuilder urlBuilder,
        ILogger<CoverCacheService> logger)
    {
        _http = http;
        _session = session;
        _connection = connection;
        _urlBuilder = urlBuilder;
        _logger = logger;
    }

    public async Task<Bitmap?> GetCoverAsync(string songId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(songId, out var cached))
        {
            return cached;
        }

        SyncConnectionFromSession();
        var sid = _session.SessionId;
        if (sid is null)
        {
            return null;
        }

        foreach (var uri in _urlBuilder.BuildCoverUris(songId, sid))
        {
            var bytes = await TryFetchCoverBytesAsync(uri, cancellationToken).ConfigureAwait(false);
            if (bytes is null || bytes.Length == 0 || !LooksLikeImage(bytes))
            {
                continue;
            }

            try
            {
                await using var memory = new MemoryStream(bytes, writable: false);
                var bitmap = await CreateBitmapOnUiThreadAsync(memory).ConfigureAwait(false);
                if (bitmap is not null)
                {
                    _cache[songId] = bitmap;
                    return bitmap;
                }
            }
            catch (IOException)
            {
                // 尝试下一个 URL
            }
        }

        return null;
    }

    private async Task<byte[]?> TryFetchCoverBytesAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.UserAgent.ParseAdd("AvMusic/1.0 (Synology Audio Station)");

            using var response = await _http.Client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "封面请求失败 {Uri}", uri);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "封面尝试失败 {Uri}", uri);
            return null;
        }
    }

    private void SyncConnectionFromSession()
    {
        if (_session.Server is not null)
        {
            _connection.Server = _session.Server;
        }
    }

    private static async Task<Bitmap?> CreateBitmapOnUiThreadAsync(MemoryStream stream)
    {
        stream.Position = 0;

        if (Dispatcher.UIThread.CheckAccess())
        {
            return CreateBitmap(stream);
        }

        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            stream.Position = 0;
            return CreateBitmap(stream);
        });
    }

    private static Bitmap? CreateBitmap(Stream stream)
    {
        try
        {
            return new Bitmap(stream);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static bool LooksLikeImage(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return false;
        }

        if (data[0] == 0xFF && data[1] == 0xD8)
        {
            return true;
        }

        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            return true;
        }

        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
        {
            return true;
        }

        return data.Length >= 12
            && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50;
    }
}
