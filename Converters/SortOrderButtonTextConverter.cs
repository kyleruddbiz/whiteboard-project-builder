using Microsoft.UI.Xaml.Data;

namespace WhiteboardProjectBuilder.Converters;

public class SortOrderButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSortDescending)
        {
            return isSortDescending ? "Sort Ascending" : "Sort Descending";
        }
        return "Sort Descending";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
