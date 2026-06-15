using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvMusic.Ui;

namespace AvMusic.Controls;

/// <summary>
/// 统一矢量图标控件。
/// </summary>
public partial class AppIcon : UserControl
{
    public static readonly StyledProperty<AppIconKind> KindProperty =
        AvaloniaProperty.Register<AppIcon, AppIconKind>(nameof(Kind));

    public static readonly StyledProperty<bool> IsFilledProperty =
        AvaloniaProperty.Register<AppIcon, bool>(nameof(IsFilled));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<AppIcon, double>(nameof(IconSize), 20);

    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<AppIcon, IBrush?>(nameof(IconBrush));

    static AppIcon()
    {
        KindProperty.Changed.AddClassHandler<AppIcon>((icon, _) => icon.UpdateIcon());
        IsFilledProperty.Changed.AddClassHandler<AppIcon>((icon, _) => icon.UpdateIcon());
        IconBrushProperty.Changed.AddClassHandler<AppIcon>((icon, _) => icon.UpdateIcon());
        ForegroundProperty.Changed.AddClassHandler<AppIcon>((icon, _) => icon.UpdateIcon());
        IconSizeProperty.Changed.AddClassHandler<AppIcon>((icon, e) =>
        {
            icon.Width = (double)e.NewValue!;
            icon.Height = (double)e.NewValue!;
        });
    }

    public AppIcon()
    {
        InitializeComponent();
        Width = IconSize;
        Height = IconSize;
        UpdateIcon();
    }

    public AppIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public bool IsFilled
    {
        get => GetValue(IsFilledProperty);
        set => SetValue(IsFilledProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    private void UpdateIcon()
    {
        if (IconPath is null)
        {
            return;
        }

        try
        {
            var geometry = AppIconGeometry.GetGeometry(Kind, IsFilled);
            IconPath.Data = geometry;
            IconPath.Fill = IconBrush ?? Foreground;
            IconPath.IsVisible = geometry is not null;
        }
        catch
        {
            IconPath.IsVisible = false;
        }
    }
}
