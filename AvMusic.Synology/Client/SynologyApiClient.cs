using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AvMusic.Core.Models;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AvMusic.Synology.Client;

/// <summary>
/// 群晖 Web API HTTP 封装：表单 POST、会话参数、证书策略。
/// </summary>
public sealed class SynologyApiClient : ISynologyApiClient, IDisposable
{
    private readonly ISynologyConnectionContext _connection;
    private readonly IOptionsMonitor<SynologyClientOptions> _options;
    private readonly ILogger<SynologyApiClient> _logger;
    private readonly HttpClient _httpClient;

    public SynologyApiClient(
        ISynologyConnectionContext connection,
        IOptionsMonitor<SynologyClientOptions> options,
        ILogger<SynologyApiClient> logger)
    {
        _connection = connection;
        _options = options;
        _logger = logger;
        _httpClient = CreateHttpClient();
        _options.OnChange(_ => RecreateHandlerIfNeeded());
    }

    public bool IsConfigured => _connection.Server is not null;

    public async Task<string> PostAsync(
        string cgiPath,
        IReadOnlyDictionary<string, string> parameters,
        string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.Server is null)
        {
            throw new InvalidOperationException("尚未配置群晖服务器地址，请先设置 ServerProfile。");
        }

        var path = cgiPath.TrimStart('/');
        var url = $"{_connection.Server.WebApiBaseUrl}/{path}";

        using var content = new FormUrlEncodedContent(BuildFormParameters(parameters, sessionId));
        _logger.LogDebug("POST {Url}", url);

        using var response = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public Uri BuildAuthenticatedUri(
        string cgiPath,
        IReadOnlyDictionary<string, string> queryParameters,
        string sessionId)
    {
        if (_connection.Server is null)
        {
            throw new InvalidOperationException("尚未配置群晖服务器地址。");
        }

        var query = new Dictionary<string, string>(queryParameters)
        {
            ["_sid"] = sessionId
        };

        var qs = string.Join("&",
            query.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        var path = cgiPath.TrimStart('/');
        return new Uri($"{_connection.Server.WebApiBaseUrl}/{path}?{qs}");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private void RecreateHandlerIfNeeded()
    {
        _httpClient.Timeout = _options.CurrentValue.RequestTimeout;
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildFormParameters(
        IReadOnlyDictionary<string, string> parameters,
        string? sessionId)
    {
        foreach (var pair in parameters)
        {
            yield return pair;
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            yield return new KeyValuePair<string, string>("_sid", sessionId);
        }
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = ValidateServerCertificate
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            Timeout = _options.CurrentValue.RequestTimeout,
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
            }
        };
    }

    private bool ValidateServerCertificate(
        HttpRequestMessage _,
        X509Certificate2? __,
        X509Chain? ___,
        SslPolicyErrors errors)
    {
        if (_connection.Server?.TrustAllCertificates == true)
        {
            return true;
        }

        return errors == SslPolicyErrors.None;
    }
}
