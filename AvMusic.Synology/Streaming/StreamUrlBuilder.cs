using AvMusic.Core.Models;
using AvMusic.Synology.Client;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Services;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Streaming;

/// <summary>
/// 根据 API 定义构建 Audio Station 流媒体 URL。
/// </summary>
public sealed class StreamUrlBuilder
{
    private const string StreamApiName = "SYNO.AudioStation.Stream";
    private const string DownloadApiName = "SYNO.AudioStation.Download";
    private const string CoverApiName = "SYNO.AudioStation.Cover";

    private readonly ISynologyApiClient _client;
    private readonly ISynologyConnectionContext _connection;
    private readonly IApiInfoService _apiInfo;
    private readonly ILogger<StreamUrlBuilder> _logger;

    public StreamUrlBuilder(
        ISynologyApiClient client,
        ISynologyConnectionContext connection,
        IApiInfoService apiInfo,
        ILogger<StreamUrlBuilder> logger)
    {
        _client = client;
        _connection = connection;
        _apiInfo = apiInfo;
        _logger = logger;
    }

    public IReadOnlyList<StreamPlaybackCandidate> BuildPlaybackCandidates(Song song, string sessionId)
    {
        var list = new List<StreamPlaybackCandidate>();

        void Add(Uri uri, string? ext, string tag)
        {
            if (list.Any(c => c.Uri.Equals(uri)))
            {
                return;
            }

            list.Add(new StreamPlaybackCandidate(uri, ext));
            _logger.LogDebug("生成播放地址 [{Tag}]: {Uri}", tag, uri);
        }

        // README 最简形态（许多第三方客户端可用）
        Add(BuildSimpleCgiUri("AudioStation/stream.cgi", song.Id, sessionId), null, "simple-stream");

        if (_apiInfo.TryGetApi(StreamApiName, out var streamApi))
        {
            foreach (var spec in SelectPlaybackSources(song))
            {
                var candidate = BuildStreamCandidate(song.Id, sessionId, spec, streamApi);
                if (candidate is not null)
                {
                    Add(candidate.Uri, candidate.PreferredExtension, $"api-{spec.Method}");
                }
            }
        }

        var download = BuildDownloadCandidate(song.Id, sessionId);
        if (download is not null)
        {
            Add(download.Uri, download.PreferredExtension, "download");
        }

        return list;
    }

    public IReadOnlyList<Uri> BuildCoverUris(string songId, string sessionId)
    {
        var list = new List<Uri>();

        void Add(Uri uri, string tag)
        {
            if (list.Contains(uri))
            {
                return;
            }

            list.Add(uri);
            _logger.LogDebug("生成封面地址 [{Tag}]: {Uri}", tag, uri);
        }

        Add(BuildSimpleCgiUri("AudioStation/cover.cgi", songId, sessionId), "simple");

        if (!_apiInfo.TryGetApi(CoverApiName, out var coverApi))
        {
            return list;
        }

        foreach (var method in new[] { "getsongcover", "getcover" })
        {
            Add(_client.BuildAuthenticatedUri(
                coverApi.Path,
                new Dictionary<string, string>
                {
                    ["api"] = CoverApiName,
                    ["version"] = coverApi.MaxVersion.ToString(),
                    ["method"] = method,
                    ["id"] = songId
                },
                sessionId), method);
        }

        // 仅 id，无 method（部分 DSM 版本）
        Add(_client.BuildAuthenticatedUri(
            coverApi.Path,
            new Dictionary<string, string>
            {
                ["api"] = CoverApiName,
                ["version"] = coverApi.MinVersion.ToString(),
                ["id"] = songId
            },
            sessionId), "id-only");

        return list;
    }

    public Uri BuildCoverUri(string songId, string sessionId) =>
        BuildCoverUris(songId, sessionId).FirstOrDefault()
        ?? BuildSimpleCgiUri("AudioStation/cover.cgi", songId, sessionId);

    private Uri BuildSimpleCgiUri(string cgiPath, string songId, string sessionId)
    {
        var server = _connection.Server
            ?? throw new InvalidOperationException("尚未配置群晖服务器地址。");

        var path = cgiPath.TrimStart('/');
        var qs = $"id={Uri.EscapeDataString(songId)}&_sid={Uri.EscapeDataString(sessionId)}";
        return new Uri($"{server.WebApiBaseUrl}/{path}?{qs}");
    }

    private StreamPlaybackCandidate? BuildStreamCandidate(
        string songId,
        string sessionId,
        PlaybackSourceSpec spec,
        Json.SynologyApiDefinition streamApi)
    {
        var versions = spec.Version is int v
            ? new[] { v }
            : new[] { streamApi.MaxVersion, streamApi.MinVersion }.Distinct();

        foreach (var version in versions)
        {
            var path = string.IsNullOrEmpty(spec.PathSuffix)
                ? streamApi.Path
                : $"{streamApi.Path.TrimEnd('/')}/{spec.PathSuffix}";

            var uri = _client.BuildAuthenticatedUri(
                path,
                new Dictionary<string, string>
                {
                    ["api"] = StreamApiName,
                    ["version"] = version.ToString(),
                    ["method"] = spec.Method!,
                    ["id"] = songId
                },
                sessionId);

            return new StreamPlaybackCandidate(
                uri,
                PreferredExtension: spec.Method == "transcode" ? ".mp3" : null);
        }

        return null;
    }

    private StreamPlaybackCandidate? BuildDownloadCandidate(string songId, string sessionId)
    {
        if (!_apiInfo.TryGetApi(DownloadApiName, out var api))
        {
            return null;
        }

        var uri = _client.BuildAuthenticatedUri(
            api.Path,
            new Dictionary<string, string>
            {
                ["api"] = DownloadApiName,
                ["version"] = api.MaxVersion.ToString(),
                ["method"] = "download",
                ["id"] = songId
            },
            sessionId);

        return new StreamPlaybackCandidate(uri, PreferredExtension: null);
    }

    private static IEnumerable<PlaybackSourceSpec> SelectPlaybackSources(Song song)
    {
        if (!IsMp3(song.Codec))
        {
            yield return new(PlaybackSourceKind.Stream, "transcode", "0.mp3", null);
            yield return new(PlaybackSourceKind.Stream, "transcode", null, null);
        }

        yield return new(PlaybackSourceKind.Stream, "stream", null, null);
    }

    private static bool IsMp3(string? codec) =>
        !string.IsNullOrWhiteSpace(codec)
        && codec.Contains("mp3", StringComparison.OrdinalIgnoreCase);

    private enum PlaybackSourceKind
    {
        Stream,
        Download
    }

    private readonly record struct PlaybackSourceSpec(
        PlaybackSourceKind Kind,
        string? Method,
        string? PathSuffix,
        int? Version);
}

public sealed record StreamPlaybackCandidate(Uri Uri, string? PreferredExtension);
