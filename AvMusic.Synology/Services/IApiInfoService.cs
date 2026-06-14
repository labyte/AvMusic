using AvMusic.Synology.Json;

namespace AvMusic.Synology.Services;

public interface IApiInfoService
{
    /// <summary>
    /// 拉取 DSM 可用 API 列表并缓存。
    /// </summary>
    Task<IReadOnlyDictionary<string, SynologyApiDefinition>> GetApisAsync(
        CancellationToken cancellationToken = default);

    SynologyApiDefinition GetRequiredApi(string apiName);

    bool TryGetApi(string apiName, out SynologyApiDefinition? definition);
}
