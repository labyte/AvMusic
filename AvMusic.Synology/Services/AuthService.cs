using System.Text.Json;
using AvMusic.Core.Models;
using AvMusic.Core.Session;
using AvMusic.Synology.Client;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Json;
using Microsoft.Extensions.Logging;

namespace AvMusic.Synology.Services;

public sealed class AuthService : IAuthService
{
    private const string AuthApiName = "SYNO.API.Auth";

    private readonly ISynologyApiClient _client;
    private readonly ISynologyConnectionContext _connection;
    private readonly ISessionState _session;
    private readonly IApiInfoService _apiInfo;
    private readonly ISynologyMediaHttpClient _mediaHttp;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ISynologyApiClient client,
        ISynologyConnectionContext connection,
        ISessionState session,
        IApiInfoService apiInfo,
        ISynologyMediaHttpClient mediaHttp,
        ILogger<AuthService> logger)
    {
        _client = client;
        _connection = connection;
        _session = session;
        _apiInfo = apiInfo;
        _mediaHttp = mediaHttp;
        _logger = logger;
    }

    public async Task LoginAsync(
        ServerProfile server,
        string account,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        _connection.Server = server;
        _mediaHttp.Reset();

        await _apiInfo.GetApisAsync(cancellationToken).ConfigureAwait(false);
        var authApi = _apiInfo.GetRequiredApi(AuthApiName);

        var json = await _client.PostAsync(
            authApi.Path,
            new Dictionary<string, string>
            {
                ["api"] = AuthApiName,
                ["method"] = "login",
                ["version"] = authApi.MaxVersion.ToString(),
                ["account"] = account,
                ["passwd"] = password,
                ["session"] = "AudioStation",
                ["format"] = "sid"
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var response = JsonSerializer.Deserialize<SynologyResponse<LoginData>>(json, SynologyJsonDefaults.Options);

        if (response is not { Success: true, Data.Sid: { Length: > 0 } sid })
        {
            var code = response?.Error?.Code ?? -1;
            _logger.LogWarning("登录失败，错误码 {Code}", code);
            throw new SynologyApiException("登录失败，请检查地址、账号或密码", code);
        }

        _session.SetSession(server, sid);
        _logger.LogInformation("已登录群晖 {Host}，用户 {Account}", server.Host, account);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (!_session.IsAuthenticated || _session.Server is null)
        {
            _session.Clear();
            return;
        }

        try
        {
            var authApi = _apiInfo.GetRequiredApi(AuthApiName);
            await _client.PostAsync(
                authApi.Path,
                new Dictionary<string, string>
                {
                    ["api"] = AuthApiName,
                    ["method"] = "logout",
                    ["version"] = authApi.MaxVersion.ToString(),
                    ["session"] = "AudioStation"
                },
                _session.SessionId,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "登出请求失败，将清除本地会话");
        }
        finally
        {
            _session.Clear();
            _mediaHttp.Reset();
        }
    }
}
