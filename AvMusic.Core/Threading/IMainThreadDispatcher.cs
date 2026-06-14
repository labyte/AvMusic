namespace AvMusic.Core.Threading;

/// <summary>
/// 将调用切回 UI 线程（LibVLC 在部分平台需在 UI 线程操作）。
/// </summary>
public interface IMainThreadDispatcher
{
    void Post(Action action);

    Task InvokeAsync(Func<Task> action);
}
