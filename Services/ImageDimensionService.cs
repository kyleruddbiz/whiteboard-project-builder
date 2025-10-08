using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for retrieving image dimensions without loading the full image into memory.
/// </summary>
public class ImageDimensionService
{
    /// <summary>
    /// Gets image dimensions from a URI by reading file metadata only.
    /// Supports ms-appx:// and ms-appdata:// URIs.
    /// </summary>
    /// <param name="imageUri">URI of the image file</param>
    /// <returns>Tuple of (width, height) in pixels</returns>
    public async Task<(uint width, uint height)> GetImageDimensionsAsync(string imageUri)
    {
        try
        {
            // Handle relative paths by converting to ms-appx:// URI
            string fullUri = imageUri.Contains("://") ? imageUri : $"ms-appx:///{imageUri}";

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(fullUri));
            var properties = await file.Properties.GetImagePropertiesAsync();

            return (properties.Width, properties.Height);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get image dimensions from URI: {imageUri}", ex);
        }
    }
}
