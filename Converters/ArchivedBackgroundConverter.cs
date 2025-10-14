using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WhiteboardProjectBuilder.Converters;

public static class BackgroundColorHelper
{
    public static SolidColorBrush GetBackgroundBrush(bool isArchived, bool hasError)
    {
        if (isArchived && hasError)
        {
            // Blended color: average of tan (245, 222, 179) and light red (255, 220, 220)
            return new SolidColorBrush(Color.FromArgb(255, 250, 221, 200));
        }
        else if (isArchived)
        {
            // Light tan for archived
            return new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));
        }
        else if (hasError)
        {
            // Light red for error state (empty title or default image)
            return new SolidColorBrush(Color.FromArgb(255, 255, 220, 220));
        }

        // White for normal
        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    }
}
