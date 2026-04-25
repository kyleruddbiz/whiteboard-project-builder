using Microsoft.UI.Xaml;

namespace WhiteboardProjectBuilder.Constants;

/// <summary>
/// Single source of truth for the image-area layout. The default-transform math
/// reads these so it stays in sync with the borders that visually shrink the
/// rendered image area. XAML consumes the values via <see cref="ImageLayoutXamlConstants"/>.
/// </summary>
public static class ImageLayoutConstants
{
    public const double ImageViewportBorder = 2;
    public static readonly Thickness ImageViewportBorderThickness = new(ImageViewportBorder);

    public static class Project
    {
        public const double OuterBorder = 4;
        public const double Width = 420;
        public const double Height = 360;
        public const double ClipWidth = Width - 2 * (OuterBorder + ImageViewportBorder);
        public const double ClipHeight = Height - 2 * (OuterBorder + ImageViewportBorder);
        public static readonly Thickness OuterBorderThickness = new(OuterBorder);
    }

    public static class Task
    {
        public const double Width = 420;
        public const double Height = 240;
        public const double ClipWidth = Width - 2 * ImageViewportBorder;
        public const double ClipHeight = Height - 2 * ImageViewportBorder;
    }
}
