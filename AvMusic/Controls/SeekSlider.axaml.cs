using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvMusic.ViewModels;
using AvMusic.Views;

namespace AvMusic.Controls;

/// <summary>
/// 播放进度条：底层连续轨道 + 上层透明 Slider 交互。
/// </summary>
public partial class SeekSlider : UserControl
{
    public static readonly StyledProperty<double> MinimumProperty =
        Slider.MinimumProperty.AddOwner<SeekSlider>();

    public static readonly StyledProperty<double> MaximumProperty =
        Slider.MaximumProperty.AddOwner<SeekSlider>();

    public static readonly StyledProperty<double> ValueProperty =
        Slider.ValueProperty.AddOwner<SeekSlider>();

    private Grid? _trackLayer;
    private Border? _trackProgress;
    private Slider? _slider;

    public SeekSlider()
    {
        InitializeComponent();
        _trackLayer = this.FindControl<Grid>("PART_TrackLayer");
        _trackProgress = this.FindControl<Border>("PART_TrackProgress");
        _slider = this.FindControl<Slider>("PART_Slider");

        this.GetObservable(ValueProperty).Subscribe(_ => UpdateProgressWidth());
        this.GetObservable(MaximumProperty).Subscribe(_ => UpdateProgressWidth());
        this.GetObservable(MinimumProperty).Subscribe(_ => UpdateProgressWidth());

        Loaded += OnLoaded;
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_trackLayer is not null)
        {
            _trackLayer.SizeChanged += (_, _) => UpdateProgressWidth();
        }

        if (_slider is not null)
        {
            _slider.GetObservable(Slider.ValueProperty).Subscribe(_ => UpdateProgressWidth());
            SeekSliderBehavior.Attach(_slider, () => DataContext as PlayerViewModel, this);
        }

        UpdateProgressWidth();
        Dispatcher.UIThread.Post(UpdateProgressWidth);
    }

    private void UpdateProgressWidth()
    {
        if (_trackLayer is null || _trackProgress is null)
        {
            return;
        }

        var width = _trackLayer.Bounds.Width;
        if (width <= 0)
        {
            return;
        }

        var range = Maximum - Minimum;
        var ratio = range > 0 ? (Value - Minimum) / range : 0;
        ratio = Math.Clamp(ratio, 0, 1);
        _trackProgress.Width = width * ratio;
    }
}
