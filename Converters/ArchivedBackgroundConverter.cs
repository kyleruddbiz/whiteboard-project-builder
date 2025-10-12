using Microsoft.UI.Xaml.Media;

namespace WhiteboardProjectBuilder.Converters;

public static class BackgroundColorHelper
{
    public static SolidColorBrush GetBackgroundBrush(bool isArchived, bool isUsingTemporaryImage)
    {
        if (isArchived && isUsingTemporaryImage)
        {
            // Blended color: average of tan (245, 222, 179) and light red (255, 220, 220)
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 250, 221, 200));
        }
        else if (isArchived)
        {
            // Light tan for archived
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 222, 179));
        }
        else if (isUsingTemporaryImage)
        {
            // Light red for temporary image
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 220, 220));
        }

        // White for normal
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    }
}
