namespace WhiteboardProjectBuilder.Constants;

/// <summary>
/// XAML aliases for <see cref="WhiteboardItemSizes"/>. <c>x:Bind</c> can't traverse
/// nested static classes in path expressions, so per-size dimensions are re-exported
/// as top-level constants. C# code should reference <see cref="WhiteboardItemSizes"/>.
/// </summary>
public static class WhiteboardItemSizesXamlConstants
{
    public const double SmallWidth = WhiteboardItemSizes.Small.Width;
    public const double SmallHeight = WhiteboardItemSizes.Small.Height;

    public const double MediumWidth = WhiteboardItemSizes.Medium.Width;
    public const double MediumHeight = WhiteboardItemSizes.Medium.Height;

    public const double LargeWidth = WhiteboardItemSizes.Large.Width;
    public const double LargeHeight = WhiteboardItemSizes.Large.Height;

    public const double HugeWidth = WhiteboardItemSizes.Huge.Width;
    public const double HugeHeight = WhiteboardItemSizes.Huge.Height;
}
