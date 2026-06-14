using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AvMusic.Synology.Client;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Streaming;

public sealed class SynologyStreamDownloader
{
    private readonly ISynologyMediaHttpClient _http;
    private readonly ILogger<SynologyStreamDownloader> _logger;
    private readonly string _cacheDir;

    public SynologyStreamDownloader(
        ISynologyMediaHttpClient http,
        ILogger<SynologyStreamDownloader> logger)
    {
        _http = http;
        _logger = logger;
        _cacheDir = Path.Combine(Path.GetTempPath(), "AvMusic", "streams");
        Directory.CreateDirectory(_cacheDir);
    }

    public Task<StreamDownloadResult> DownloadAsync(
        Uri remoteUri,
        string songId,
        string? preferredExtension,
        CancellationToken cancellationToken = default) =>
        DownloadInternalAsync(remoteUri, songId, preferredExtension, cancellationToken);

    private async Task<StreamDownloadResult> DownloadInternalAsync(
        Uri remoteUri,
        string songId,
        string? preferredExtension,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, remoteUri);
            request.Headers.UserAgent.ParseAdd("AvMusic/1.0 (Synology Audio Station)");

            using var response = await _http.StreamClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await ReadSnippetAsync(response, cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("拉流 HTTP {Status}: {Uri} Body={Body}", response.StatusCode, remoteUri, errBody);
                return new StreamDownloadResult(null, (int)response.StatusCode, errBody);
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("拉流返回 JSON: {Uri} {Json}", remoteUri, Truncate(json, 200));
                return new StreamDownloadResult(null, (int)response.StatusCode, Truncate(json, 200));
            }

            var extension = ResolveExtension(preferredExtension, mediaType, remoteUri);
            var filePath = Path.Combine(_cacheDir, $"{SanitizeFileName(songId)}_{Guid.NewGuid():N}{extension}");

            await using (var network = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var file = File.Create(filePath))
            {
                await network.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
            }

            if (await IsSynologyErrorJsonAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                TryDelete(filePath);
                return new StreamDownloadResult(null, (int)response.StatusCode, "NAS 返回错误 JSON");
            }

            var length = new FileInfo(filePath).Length;
            if (length < 1024)
            {
                TryDelete(filePath);
                return new StreamDownloadResult(null, (int)response.StatusCode, $"文件过小 ({length} 字节)");
            }

            _logger.LogInformation("已缓存音频 {SongId} -> {Path} ({Length} 字节)", songId, filePath, length);
            return new StreamDownloadResult(filePath, (int)response.StatusCode, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return new StreamDownloadResult(null, null, "下载已取消");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "拉流 HTTP 失败: {Uri}", remoteUri);
            return new StreamDownloadResult(null, null, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "拉流失败: {Uri}", remoteUri);
            return new StreamDownloadResult(null, null, ex.Message);
        }
    }

    public void Delete(string? filePath) => TryDelete(filePath);

    private static async Task<string?> ReadSnippetAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return Truncate(text, 200);
        }
        catch
        {
            return null;
        }
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max];

    private static string ResolveExtension(string? preferred, string mediaType, Uri uri)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred.StartsWith('.') ? preferred : $".{preferred}";
        }

        if (mediaType.Contains("mpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ".mp3";
        }

        if (mediaType.Contains("flac", StringComparison.OrdinalIgnoreCase))
        {
            return ".flac";
        }

        var ext = Path.GetExtension(uri.AbsolutePath);
        return string.IsNullOrEmpty(ext) ? ".audio" : ext;
    }

    private static async Task<bool> IsSynologyErrorJsonAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        var buffer = new byte[Math.Min(512, stream.Length)];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        if (read == 0 || buffer[0] != (byte)'{')
        {
            return false;
        }

        try
        {
            var text = Encoding.UTF8.GetString(buffer, 0, read);
            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.TryGetProperty("success", out var success)
                && success.ValueKind == JsonValueKind.False;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string SanitizeFileName(string id) =>
        string.Concat(id.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // 忽略
        }
    }
}
