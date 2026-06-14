using AvMusic.Synology;
using AvMusic.Synology.Services;
using Avalonia.Threading;
using ReactiveUI;

namespace AvMusic.ViewModels.Library;

public abstract class LibraryPageBase : ViewModelBase, ILibraryPage
{
    private bool _isLoading;
    private string? _errorMessage;
    private int _loadGeneration;
    private CancellationTokenSource? _loadCts;

    protected LibraryPageBase(IAudioStationService audioStation, IAuthService authService)
    {
        AudioStation = audioStation;
        AuthService = authService;
    }

    protected IAudioStationService AudioStation { get; }

    protected IAuthService AuthService { get; }

    public abstract string Title { get; }

    public bool IsLoading
    {
        get => _isLoading;
        protected set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public abstract Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>开始新的加载会话（取消上一次），返回合并后的取消令牌。</summary>
    protected CancellationToken BeginLoadSession(CancellationToken externalToken = default)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        Interlocked.Increment(ref _loadGeneration);
        return _loadCts.Token;
    }

    protected bool IsCurrentLoadSession(int generation) => generation == _loadGeneration;

    protected async Task RunSafeAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var generation = _loadGeneration;

        await DispatchUiAsync(() =>
        {
            IsLoading = true;
            ErrorMessage = null;
        }).ConfigureAwait(true);

        try
        {
            await action(cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 导航重复点击或切换页面时取消
        }
        catch (SynologyApiException ex) when (ex.ErrorCode is 106 or 119)
        {
            if (!IsCurrentLoadSession(generation))
            {
                return;
            }

            await DispatchUiAsync(() => ErrorMessage = "会话已过期，请重新登录。").ConfigureAwait(true);
            await AuthService.LogoutAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (SynologyApiException ex)
        {
            if (IsCurrentLoadSession(generation))
            {
                await DispatchUiAsync(() => ErrorMessage = ex.Message).ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            if (IsCurrentLoadSession(generation))
            {
                await DispatchUiAsync(() => ErrorMessage = ex.Message).ConfigureAwait(true);
            }
        }
        finally
        {
            if (IsCurrentLoadSession(generation))
            {
                await DispatchUiAsync(() => IsLoading = false).ConfigureAwait(true);
            }
        }
    }

    protected static async Task DispatchUiAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }

    protected static async Task DispatchUiAsync(Func<Task> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            await action().ConfigureAwait(true);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }
}
