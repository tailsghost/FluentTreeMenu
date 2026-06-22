using System.Globalization;
using System.Windows.Data;

namespace FluentTreeMenu.Converts;

public class ConnectorLineHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
        {
            return 0d;
        }

        if (values[0] is not (bool and true) || parameter is not string par) return 0d;
        if(double.TryParse(par, out var parResult))
            return parResult;

        return 0d;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

