using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WhiteboardProjectBuilder.Converters;

public static class BackgroundColorHelper
{
    public static SolidColorBrush GetBackgroundBrush(bool isArchived)
    {
        if (isArchived)
        {
            // Light tan for archived
            return new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));
        }

        // White for normal
        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    }
}
