using System.Text.Json;
using AvMusic.Synology.Client;
using AvMusic.Synology.Json;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Services;

public sealed class ApiInfoService : IApiInfoService
{
    private readonly ISynologyApiClient _client;
    private readonly ILogger<ApiInfoService> _logger;
    private IReadOnlyDictionary<string, SynologyApiDefinition>? _cache;

    public ApiInfoService(ISynologyApiClient client, ILogger<ApiInfoService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<string, SynologyApiDefinition>> GetApisAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var json = await _client.PostAsync(
            "entry.cgi",
            new Dictionary<string, string>
            {
                ["api"] = "SYNO.API.Info",
                ["method"] = "query",
                ["version"] = "1",
                ["query"] = "all"
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var response = JsonSerializer.Deserialize<SynologyResponse<Dictionary<string, SynologyApiDefinition>>>(
            json, SynologyJsonDefaults.Options);

        if (response is not { Success: true } || response.Data is null)
        {
            var code = response?.Error?.Code;
            throw new SynologyApiException("获取 API 信息失败", code ?? -1);
        }

        _cache = response.Data;
        _logger.LogInformation("已缓存 {Count} 个群晖 API 定义", _cache.Count);
        return _cache;
    }

    public SynologyApiDefinition GetRequiredApi(string apiName)
    {
        if (!TryGetApi(apiName, out var definition) || definition is null)
        {
            throw new InvalidOperationException($"未找到 API 定义: {apiName}，请先调用 GetApisAsync。");
        }

        return definition;
    }

    public bool TryGetApi(string apiName, out SynologyApiDefinition? definition)
    {
        if (_cache is null)
        {
            definition = null;
            return false;
        }

        return _cache.TryGetValue(apiName, out definition);
    }
}
