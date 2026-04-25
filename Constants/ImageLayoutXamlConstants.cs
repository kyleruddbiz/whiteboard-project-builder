using Microsoft.UI.Xaml;

namespace WhiteboardProjectBuilder.Constants;

/// <summary>
/// XAML-facing aliases for <see cref="ImageLayoutConstants"/>. WinUI 3's
/// <c>x:Bind</c> can't traverse nested static classes in path expressions, so
/// the inner-class values are re-exported here at top level for XAML to bind to.
/// All values are aliases — no duplicated literals. C# code should reference
/// <see cref="ImageLayoutConstants"/> directly.
/// </summary>
public static class ImageLayoutXamlConstants
{
    public static readonly Thickness ImageViewportBorderThickness = ImageLayoutConstants.ImageViewportBorderThickness;

    public const double ProjectWidth = ImageLayoutConstants.Project.Width;
    public const double ProjectHeight = ImageLayoutConstants.Project.Height;
    public static readonly Thickness ProjectOuterBorderThickness = ImageLayoutConstants.Project.OuterBorderThickness;

    public const double TaskWidth = ImageLayoutConstants.Task.Width;
    public const double TaskHeight = ImageLayoutConstants.Task.Height;
}
