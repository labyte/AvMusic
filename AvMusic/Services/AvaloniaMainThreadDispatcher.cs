using AvMusic.Core.Threading;

namespace AvMusic.Services;

public sealed class AvaloniaMainThreadDispatcher : IMainThreadDispatcher
{
    public void Post(Action action) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(action);

    public Task InvokeAsync(Func<Task> action) =>
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(action);
}
