using System.Globalization;
using Avalonia.Data.Converters;

namespace AvMusic.Converters;

/// <summary>
/// 将非零整数转换为 true（如队列数量 > 0 显示徽标）。
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public static readonly IntToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int i && i > 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
