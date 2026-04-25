namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for calculating image transform parameters.
/// </summary>
/// <remarks>
/// Math assumes the rendering chain in <c>ImageViewport.xaml</c>:
/// outer Grid with TranslateTransform (centred origin), Image with Stretch=Uniform
/// and ScaleTransform (centred origin). At ZoomFactor=1 the image is at Stretch=Uniform fit.
/// Offsets are pixel translations of the image's centre relative to the viewport's centre.
/// </remarks>
public class ImageTransformService
{
    public const double MinZoomFactor = 1.0;

    /// <summary>
    /// Centred UniformToFill: the ZoomFactor that makes the Stretch=Uniform image fill
    /// the viewport in both axes (cropping the longer axis), with zero offset.
    /// </summary>
    public (double zoomFactor, double offsetX, double offsetY) CalculateDefaultTransform(
        double imageWidth, double imageHeight,
        double viewportWidth, double viewportHeight)
    {
        var (uniformWidth, uniformHeight) = CalculateUniformFitSize(imageWidth, imageHeight, viewportWidth, viewportHeight);
        double zoomFactor = Math.Max(viewportWidth / uniformWidth, viewportHeight / uniformHeight);
        return (zoomFactor, 0, 0);
    }

    /// <summary>
    /// Displayed image dimensions at ZoomFactor=1 (Stretch=Uniform fit inside the viewport).
    /// </summary>
    public (double width, double height) CalculateUniformFitSize(
        double imageWidth, double imageHeight,
        double viewportWidth, double viewportHeight)
    {
        double uniformScale = Math.Min(viewportWidth / imageWidth, viewportHeight / imageHeight);
        return (imageWidth * uniformScale, imageHeight * uniformScale);
    }

    /// <summary>
    /// Per-axis offset clamping. Enforces that at most two opposite viewport edges may show
    /// whitespace: when displayed >= viewport on an axis, the image edges may not retreat past
    /// viewport edges; when displayed &lt; viewport, the image stays inside the viewport.
    /// Both branches reduce to <c>|offset| &lt;= |displayed - viewport| / 2</c>.
    /// </summary>
    public (double offsetX, double offsetY) ClampOffset(
        double offsetX, double offsetY,
        double zoomFactor,
        double imageWidth, double imageHeight,
        double viewportWidth, double viewportHeight)
    {
        var (uniformWidth, uniformHeight) = CalculateUniformFitSize(imageWidth, imageHeight, viewportWidth, viewportHeight);
        double displayedWidth = uniformWidth * zoomFactor;
        double displayedHeight = uniformHeight * zoomFactor;
        double boundX = Math.Abs(displayedWidth - viewportWidth) / 2;
        double boundY = Math.Abs(displayedHeight - viewportHeight) / 2;
        return (Math.Clamp(offsetX, -boundX, boundX), Math.Clamp(offsetY, -boundY, boundY));
    }
}
