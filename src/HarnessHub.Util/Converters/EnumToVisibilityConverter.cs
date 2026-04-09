using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HarnessHub.Util.Converters;

/// <summary>
/// enum 값을 ConverterParameter와 비교하여 Visibility로 변환한다.
/// 값이 일치하면 Visible, 아니면 Collapsed를 반환한다.
/// </summary>
public sealed class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return Visibility.Collapsed;
        }

        var enumValue = value.ToString();
        var targetValue = parameter.ToString();

        return string.Equals(enumValue, targetValue, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
