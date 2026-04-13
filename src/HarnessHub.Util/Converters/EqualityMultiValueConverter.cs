using System.Globalization;
using System.Windows.Data;

namespace HarnessHub.Util.Converters;

/// <summary>
/// 두 값을 비교하여 동일하면 true를 반환하는 MultiValueConverter.
/// 프리셋 활성 표시 등에서 Name == ActivePresetName 비교에 사용한다.
/// </summary>
public sealed class EqualityMultiValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        var first = values[0]?.ToString();
        var second = values[1]?.ToString();

        return string.Equals(first, second, StringComparison.Ordinal);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
