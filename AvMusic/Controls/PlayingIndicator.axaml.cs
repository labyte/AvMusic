using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvMusic.Controls;

/// <summary>
/// 正在播放指示器（模拟音频柱状动画）。
/// </summary>
public partial class PlayingIndicator : UserControl
{
    private readonly Border[] _bars;
    private readonly DispatcherTimer _timer;
    private int _phase;

    public PlayingIndicator()
    {
        InitializeComponent();

        _bars = [Bar1, Bar2, Bar3, Bar4];
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        _timer.Tick += (_, _) => AnimateBars();

        AttachedToVisualTree += (_, _) => _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    private void AnimateBars()
    {
        _phase = (_phase + 1) % 8;
        var heights = new[] { 4.0, 7.0, 10.0, 13.0, 10.0, 7.0, 5.0, 8.0 };

        for (var i = 0; i < _bars.Length; i++)
        {
            var index = (_phase + i) % heights.Length;
            _bars[i].Height = heights[index];
        }
    }
}
