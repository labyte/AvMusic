using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AvMusic.Synology.Connection;
using AvMusic.Synology.Options;
using Microsoft.Extensions.Options;

namespace AvMusic.Synology.Client;

public sealed class SynologyMediaHttpClient : ISynologyMediaHttpClient, IDisposable
{
    private static readonly TimeSpan StreamDownloadTimeout = TimeSpan.FromMinutes(15);

    private readonly ISynologyConnectionContext _connection;
    private readonly IOptionsMonitor<SynologyClientOptions> _options;
    private HttpClient? _client;
    private HttpClient? _streamClient;

    public SynologyMediaHttpClient(
        ISynologyConnectionContext connection,
        IOptionsMonitor<SynologyClientOptions> options)
    {
        _connection = connection;
        _options = options;
    }

    public HttpClient Client => _client ??= CreateClient(_options.CurrentValue.RequestTimeout);

    public HttpClient StreamClient => _streamClient ??= CreateClient(StreamDownloadTimeout);

    public void Reset()
    {
        _client?.Dispose();
        _streamClient?.Dispose();
        _client = null;
        _streamClient = null;
    }

    public void Dispose() => Reset();

    private HttpClient CreateClient(TimeSpan timeout)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = ValidateServerCertificate
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            Timeout = timeout
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
