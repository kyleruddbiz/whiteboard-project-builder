using Microsoft.UI.Xaml.Data;

namespace WhiteboardProjectBuilder.Converters;

public class ShowArchivedButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool showArchived)
        {
            return showArchived ? "Hide Archived" : "Show Archived";
        }
        return "Show Archived";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
