using System.Reactive;
using System.Reactive.Linq;
using AvMusic.Core.Models;
using AvMusic.Models;
using AvMusic.Services;
using AvMusic.Synology;
using AvMusic.Synology.Services;
using ReactiveUI;

namespace AvMusic.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _host = string.Empty;
    private int _port = 5001;
    private bool _useHttps = true;
    private bool _trustAllCertificates = true;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberCredentials = true;
    private bool _isBusy;
    private string? _errorMessage;

    public LoginViewModel(
        IAuthService authService,
        IServerSettingsStore settingsStore)
    {
        AuthService = authService;
        SettingsStore = settingsStore;

        var canLogin = this.WhenAnyValue(
            x => x.Host,
            x => x.Username,
            x => x.Password,
            x => x.IsBusy,
            (host, user, pass, busy) =>
                !busy
                && !string.IsNullOrWhiteSpace(host)
                && !string.IsNullOrWhiteSpace(user)
                && !string.IsNullOrWhiteSpace(pass));

        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);

        _ = LoadSavedSettingsAsync();
    }

    public IAuthService AuthService { get; }

    public IServerSettingsStore SettingsStore { get; }

    public string Host
    {
        get => _host;
        set => this.RaiseAndSetIfChanged(ref _host, value);
    }

    public int Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    public bool UseHttps
    {
        get => _useHttps;
        set => this.RaiseAndSetIfChanged(ref _useHttps, value);
    }

    public bool TrustAllCertificates
    {
        get => _trustAllCertificates;
        set => this.RaiseAndSetIfChanged(ref _trustAllCertificates, value);
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public bool RememberCredentials
    {
        get => _rememberCredentials;
        set => this.RaiseAndSetIfChanged(ref _rememberCredentials, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    private async Task LoadSavedSettingsAsync()
    {
        var saved = await SettingsStore.LoadAsync().ConfigureAwait(false);
        if (saved is null)
        {
            return;
        }

        Host = saved.Host;
        Port = saved.Port;
        UseHttps = saved.UseHttps;
        TrustAllCertificates = saved.TrustAllCertificates;
        Username = saved.Username;
        RememberCredentials = saved.RememberCredentials;

        if (saved.RememberCredentials)
        {
            Password = SecretProtector.Unprotect(saved.ProtectedPassword) ?? string.Empty;
        }
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var server = ServerProfileParser.Create(
                Host,
                Port,
                UseHttps,
                TrustAllCertificates);

            await AuthService.LoginAsync(server, Username.Trim(), Password).ConfigureAwait(false);

            await SettingsStore.SaveAsync(new ServerSettings
            {
                Host = server.Host,
                Port = server.Port,
                UseHttps = server.UseHttps,
                TrustAllCertificates = server.TrustAllCertificates,
                Username = Username.Trim(),
                RememberCredentials = RememberCredentials,
                ProtectedPassword = RememberCredentials
                    ? SecretProtector.Protect(Password)
                    : null
            }).ConfigureAwait(false);
        }
        catch (SynologyApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "无法连接服务器，请检查地址、端口或网络。";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"登录失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
