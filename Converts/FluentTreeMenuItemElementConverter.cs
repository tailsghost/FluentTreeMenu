using FluentTreeMenu.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace FluentTreeMenu.Converts;

public class FluentTreeMenuItemElementConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not FluentTreeMenuBase item || values[1] is not string key)
            return null!;

        return item.GetElement(key);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

