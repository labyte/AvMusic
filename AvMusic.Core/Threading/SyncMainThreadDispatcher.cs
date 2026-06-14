namespace AvMusic.Core.Threading;

/// <summary>
/// 无 UI 时的同步调度（测试或非 Avalonia 宿主）。
/// </summary>
public sealed class SyncMainThreadDispatcher : IMainThreadDispatcher
{
    public void Post(Action action) => action();

    public Task InvokeAsync(Func<Task> action) => action();
}
