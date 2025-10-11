using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace WhiteboardProjectBuilder.Converters;

public class BoolToTemporaryImageTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isUsingTemporaryImage && isUsingTemporaryImage)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
