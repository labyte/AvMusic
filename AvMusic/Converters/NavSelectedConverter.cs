using System.Globalization;
using Avalonia.Data.Converters;

namespace AvMusic.Converters;

/// <summary>
/// 判断当前导航项是否选中，用于 nav-button.selected 样式。
/// </summary>
public class NavSelectedConverter : IValueConverter
{
    public static readonly NavSelectedConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string activeKey || parameter is not string navKey)
        {
            return false;
        }

        return string.Equals(activeKey, navKey, StringComparison.Ordinal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
