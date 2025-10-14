using System.Diagnostics;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for calculating image transform parameters.
/// </summary>
public class ImageTransformService
{
    public const double ViewportWidth = 420;
    public const double ViewportHeight = 360;

    /// <summary>
    /// Calculates scale and offset to mimic UniformToFill with top-left alignment.
    /// Uses center-based coordinate system (RenderTransformOrigin="0.5,0.5").
    /// </summary>
    /// <param name="imageWidth">Width of the image in pixels</param>
    /// <param name="imageHeight">Height of the image in pixels</param>
    /// <returns>Tuple of (scale, offsetX, offsetY) for center-based transforms</returns>
    public (double scale, double offsetX, double offsetY) CalculateUniformToFillTransform(
        double imageWidth,
        double imageHeight)
    {
        // Debug output
        Debug.WriteLine($"Image dimensions: {imageWidth}x{imageHeight}");
        Debug.WriteLine($"Viewport dimensions: {ViewportWidth}x{ViewportHeight}");

        // Calculate how the image is rendered with Stretch="Uniform"
        // Uniform scales to fit INSIDE the viewport, maintaining aspect ratio
        double uniformScale = Math.Min(ViewportWidth / imageWidth, ViewportHeight / imageHeight);
        double uniformWidth = imageWidth * uniformScale;
        double uniformHeight = imageHeight * uniformScale;

        Debug.WriteLine($"After Stretch=Uniform: {uniformWidth}x{uniformHeight}, scale={uniformScale}");

        // Now calculate additional scale needed to fill viewport (UniformToFill behavior)
        // We need to scale FROM the Uniform size TO fill the viewport
        double additionalScale = Math.Max(
            ViewportWidth / uniformWidth,
            ViewportHeight / uniformHeight
        );

        Debug.WriteLine($"Additional scale needed: {additionalScale}");

        double finalScale = additionalScale; // This is applied to the already-Uniform-scaled image

        // Calculate final dimensions after both Uniform and additional scaling
        double finalWidth = uniformWidth * additionalScale;
        double finalHeight = uniformHeight * additionalScale;

        Debug.WriteLine($"Final dimensions: {finalWidth}x{finalHeight}");

        // For center-based transforms (RenderTransformOrigin="0.5,0.5"), position image at top-left
        // The viewport's center is at (0,0) in this coordinate system
        // The viewport's top-left corner is at (-ViewportWidth/2, -ViewportHeight/2)
        // The scaled image's top-left corner is at (-finalWidth/2, -finalHeight/2) relative to its center
        // To align them: offset = viewportTopLeft - imageTopLeft
        double offsetX = (-ViewportWidth / 2) - (-finalWidth / 2);
        double offsetY = (-ViewportHeight / 2) - (-finalHeight / 2);

        Debug.WriteLine($"Final transform: scale={finalScale}, offsetX={offsetX}, offsetY={offsetY}");

        return (finalScale, offsetX, offsetY);
    }
}
