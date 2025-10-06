using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WhiteboardProjectBuilder.Converters;

public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            // Check if path already has a URI scheme (ms-appdata://, ms-appx://, http://, etc.)
            if (path.Contains("://"))
            {
                return new BitmapImage(new Uri(path));
            }
            else
            {
                // Relative path - assume it's in the app package (Assets folder)
                return new BitmapImage(new Uri($"ms-appx:///{path}"));
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
