using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Constants;

/// <summary>
/// Pixel dimensions for each <see cref="WhiteboardItemSize"/>.
/// <see cref="WhiteboardItemSizesXamlConstants"/> mirrors these as XAML-bindable aliases.
/// </summary>
public static class WhiteboardItemSizes
{
    public static class Small
    {
        public const double Width = 502;
        public const double Height = 400;
    }

    public static class Medium
    {
        public const double Width = 502;
        public const double Height = 800;
    }

    public static class Large
    {
        public const double Width = 1004;
        public const double Height = 800;
    }

    public static class Huge
    {
        public const double Width = 1004;
        public const double Height = 1600;
    }

    public static double WidthOf(WhiteboardItemSize size) => size switch
    {
        WhiteboardItemSize.Small => Small.Width,
        WhiteboardItemSize.Medium => Medium.Width,
        WhiteboardItemSize.Large => Large.Width,
        WhiteboardItemSize.Huge => Huge.Width,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public static double HeightOf(WhiteboardItemSize size) => size switch
    {
        WhiteboardItemSize.Small => Small.Height,
        WhiteboardItemSize.Medium => Medium.Height,
        WhiteboardItemSize.Large => Large.Height,
        WhiteboardItemSize.Huge => Huge.Height,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };
}
