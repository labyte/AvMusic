using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using AvMusic.Ui;

namespace AvMusic.Controls;

/// <summary>
/// 图标按钮：统一尺寸、悬停态，支持双态图标切换（如播放/暂停）。
/// </summary>
public class IconButton : Button
{
    private AppIcon? _icon;

    public static readonly StyledProperty<AppIconKind> IconProperty =
        AvaloniaProperty.Register<IconButton, AppIconKind>(nameof(Icon));

    public static readonly StyledProperty<AppIconKind?> AlternateIconProperty =
        AvaloniaProperty.Register<IconButton, AppIconKind?>(nameof(AlternateIcon));

    public static readonly StyledProperty<bool> UseAlternateIconProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(UseAlternateIcon));

    public static readonly StyledProperty<bool> IconFilledProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(IconFilled));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<IconButton, double>(nameof(IconSize), 20);

    static IconButton()
    {
        IconProperty.Changed.AddClassHandler<IconButton>((b, _) => b.RefreshIcon());
        AlternateIconProperty.Changed.AddClassHandler<IconButton>((b, _) => b.RefreshIcon());
        UseAlternateIconProperty.Changed.AddClassHandler<IconButton>((b, _) => b.RefreshIcon());
        IconFilledProperty.Changed.AddClassHandler<IconButton>((b, _) => b.RefreshIcon());
        IconSizeProperty.Changed.AddClassHandler<IconButton>((b, _) => b.RefreshIcon());
    }

    public IconButton()
    {
        Classes.Add("icon-button");
        // 确保整块按钮区域可点击，而非仅图标像素
        Background = Brushes.Transparent;
        RefreshIcon();
    }

    public AppIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public AppIconKind? AlternateIcon
    {
        get => GetValue(AlternateIconProperty);
        set => SetValue(AlternateIconProperty, value);
    }

    public bool UseAlternateIcon
    {
        get => GetValue(UseAlternateIconProperty);
        set => SetValue(UseAlternateIconProperty, value);
    }

    public bool IconFilled
    {
        get => GetValue(IconFilledProperty);
        set => SetValue(IconFilledProperty, value);
    }

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        var kind = UseAlternateIcon && AlternateIcon.HasValue ? AlternateIcon.Value : Icon;

        if (_icon is null)
        {
            _icon = new AppIcon
            {
                Kind = kind,
                IsFilled = IconFilled,
                IconSize = IconSize,
                Foreground = Foreground,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Content = _icon;
        }
        else
        {
            _icon.Kind = kind;
            _icon.IsFilled = IconFilled;
            _icon.IconSize = IconSize;
            _icon.Foreground = Foreground;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ForegroundProperty)
        {
            RefreshIcon();
        }
    }
}
