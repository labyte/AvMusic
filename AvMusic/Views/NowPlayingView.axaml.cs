using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using AvMusic.Core.Models;
using AvMusic.ViewModels;
using ReactiveUI;

namespace AvMusic.Views;

public partial class NowPlayingView : UserControl
{
    private PlayerViewModel? _vm;
    private IDisposable? _indexSubscription;
    private int _previousIndex = -1;
    private const int ActiveFontSize = 20;
    private const int InactiveFontSize = 16;
    private const double ActiveOpacity = 1.0;
    private const double PastOpacity = 0.45;
    private const double FutureOpacity = 0.6;

    public NowPlayingView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _indexSubscription?.Dispose();
        _indexSubscription = null;

        _vm = DataContext as PlayerViewModel;
        if (_vm is null)
            return;

        // 监听歌词行索引变化，自动滚动
        _indexSubscription = _vm.WhenAnyValue(x => x.CurrentLyricsLineIndex)
            .Subscribe(_ => UpdateLyricsHighlight());

        // 初始高亮
        UpdateLyricsHighlight();
    }

    private void UpdateLyricsHighlight()
    {
        if (_vm is null)
            return;

        var currentIndex = _vm.CurrentLyricsLineIndex;
        var itemsControl = LyricsItemsControl;

        if (itemsControl?.ItemCount != _vm.LyricsLines.Count)
            return;

        // 更新前一个高亮行
        if (_previousIndex >= 0 && _previousIndex < itemsControl.ItemCount)
        {
            if (itemsControl.ContainerFromIndex(_previousIndex) is ContentPresenter prevPresenter)
            {
                ApplyInactiveStyle(prevPresenter, _previousIndex, _previousIndex < currentIndex);
            }
        }

        // 更新当前高亮行
        if (currentIndex >= 0 && currentIndex < itemsControl.ItemCount)
        {
            if (itemsControl.ContainerFromIndex(currentIndex) is ContentPresenter currPresenter)
            {
                ApplyActiveStyle(currPresenter);
            }
        }

        // 滚动到当前行居中
        ScrollToIndex(currentIndex);

        _previousIndex = currentIndex;
    }

    private void ApplyActiveStyle(ContentPresenter presenter)
    {
        if (presenter.Child is TextBlock tb)
        {
            tb.FontSize = ActiveFontSize;
            tb.FontWeight = FontWeight.Bold;
            tb.Opacity = ActiveOpacity;
            tb.Foreground = Brushes.White;
        }
    }

    private void ApplyInactiveStyle(ContentPresenter presenter, int index, bool isPast)
    {
        if (presenter.Child is TextBlock tb)
        {
            tb.FontSize = InactiveFontSize;
            tb.FontWeight = FontWeight.Normal;
            tb.Opacity = isPast ? PastOpacity : FutureOpacity;
            tb.Foreground = Brushes.White;
        }
    }

    private void ScrollToIndex(int index)
    {
        var scrollViewer = LyricsScrollViewer;
        var itemsControl = LyricsItemsControl;

        if (scrollViewer is null || itemsControl is null || index < 0)
            return;

        var container = itemsControl.ContainerFromIndex(index);
        if (container is null)
            return;

        // 将当前行滚动到 ScrollViewer 的中间位置
        var itemBounds = container.Bounds;
        var targetY = itemBounds.Y + itemBounds.Height / 2 - scrollViewer.Viewport.Height / 2;
        targetY = Math.Max(0, targetY);

        scrollViewer.Offset = new Avalonia.Vector(scrollViewer.Offset.X, targetY);
    }
}
