using System.Text.Json;
using AvMusic.Core.Session;
using AvMusic.Synology.Client;
using AvMusic.Synology.Json;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Services;

/// <summary>
/// Audio Station API 调用的公共 POST / 反序列化逻辑。
/// </summary>
public abstract class AudioStationServiceBase
{
    private readonly ISynologyApiClient _client;
    private readonly IApiInfoService _apiInfo;
    private readonly ISessionState _session;
    private readonly ILogger _logger;

    protected AudioStationServiceBase(
        ISynologyApiClient client,
        IApiInfoService apiInfo,
        ISessionState session,
        ILogger logger)
    {
        _client = client;
        _apiInfo = apiInfo;
        _session = session;
        _logger = logger;
    }

    protected async Task<TData> PostApiAsync<TData>(
        string apiName,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var sessionId = _session.SessionId
            ?? throw new InvalidOperationException("尚未登录，无法调用 Audio Station API。");

        var api = _apiInfo.GetRequiredApi(apiName);
        var payload = new Dictionary<string, string>(parameters)
        {
            ["api"] = apiName,
            ["version"] = api.MaxVersion.ToString()
        };

        var json = await _client.PostAsync(api.Path, payload, sessionId, cancellationToken)
            .ConfigureAwait(false);

        var response = JsonSerializer.Deserialize<SynologyResponse<TData>>(json, SynologyJsonDefaults.Options);

        if (response is not { Success: true, Data: not null })
        {
            var code = response?.Error?.Code ?? -1;
            _logger.LogWarning("API {Api} 失败，错误码 {Code}", apiName, code);
            throw new SynologyApiException($"API {apiName} 调用失败", code);
        }

        return response.Data;
    }

    protected async Task PostApiWithoutDataAsync(
        string apiName,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var sessionId = _session.SessionId
            ?? throw new InvalidOperationException("尚未登录，无法调用 Audio Station API。");

        var api = _apiInfo.GetRequiredApi(apiName);
        var payload = new Dictionary<string, string>(parameters)
        {
            ["api"] = apiName,
            ["version"] = api.MaxVersion.ToString()
        };

        var json = await _client.PostAsync(api.Path, payload, sessionId, cancellationToken)
            .ConfigureAwait(false);

        var response = JsonSerializer.Deserialize<SynologyResponse<JsonElement>>(json, SynologyJsonDefaults.Options);

        if (response is not { Success: true })
        {
            var code = response?.Error?.Code ?? -1;
            throw new SynologyApiException($"API {apiName} 调用失败", code);
        }
    }
}
