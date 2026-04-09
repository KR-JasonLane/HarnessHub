using System.Globalization;
using System.Windows.Data;

namespace HarnessHub.Util.Converters;

/// <summary>
/// enum 값을 ConverterParameter와 비교하여 bool로 변환한다.
/// 값이 일치하면 true, 아니면 false를 반환한다. RadioButton의 IsChecked 바인딩에 사용한다.
/// </summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true && parameter is not null && targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, parameter.ToString(), ignoreCase: true, out var result))
            {
                return result;
            }
        }

        return Binding.DoNothing;
    }
}
