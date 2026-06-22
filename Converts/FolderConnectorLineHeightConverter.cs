using System.Globalization;
using System.Windows.Data;

namespace FluentTreeMenu.Converts;

public class FolderConnectorLineHeightConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if(values is null || values.Length==0) return 0d;

        if (parameter is not string rowHeightStr) return 0d;

        if (!double.TryParse(rowHeightStr, out var rowHeight))
            return 0d;

        var halfRowHeight = rowHeight / 2d;

        if (values[0] is not bool hasParent) return 0d;
        if (values.Length > 3 && values[3] is bool and true)
            return halfRowHeight;

        return rowHeight;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

