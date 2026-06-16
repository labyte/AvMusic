using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AvMusic.Views;

/// <summary>
/// 滚动接近底部时自动触发加载更多。
/// </summary>
internal static class InfiniteScrollBehavior
{
    private const double Threshold = 48;

    public static void Attach(ScrollViewer scrollViewer, Func<bool> canLoadMore, Action loadMore)
    {
        var gate = false;

        void TryLoad()
        {
            if (gate || !canLoadMore())
            {
                return;
            }

            var offsetY = scrollViewer.Offset.Y;
            var viewportHeight = scrollViewer.Viewport.Height;
            var extentHeight = scrollViewer.Extent.Height;

            if (extentHeight <= 0)
            {
                return;
            }

            var reachedBottom = offsetY + viewportHeight >= extentHeight - Threshold;
            var contentNotFilled = extentHeight <= viewportHeight + Threshold;

            if (!reachedBottom && !contentNotFilled)
            {
                return;
            }

            gate = true;
            loadMore();
            Dispatcher.UIThread.Post(() => gate = false, DispatcherPriority.Background);
        }

        scrollViewer.ScrollChanged += (_, _) => TryLoad();

        scrollViewer.GetObservable(ScrollViewer.ExtentProperty).Subscribe(_ => TryLoad());

        scrollViewer.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(TryLoad, DispatcherPriority.Loaded);
    }
}
