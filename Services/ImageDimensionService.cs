using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for retrieving image dimensions without loading the full image into memory.
/// </summary>
public class ImageDimensionService
{
    /// <summary>
    /// Gets image dimensions from a URI or file path by reading file metadata only.
    /// Supports ms-appx://, ms-appdata:// URIs, and file system paths.
    /// </summary>
    /// <param name="imageUri">URI or file path of the image file</param>
    /// <returns>Tuple of (width, height) in pixels</returns>
    public async Task<(uint width, uint height)> GetImageDimensionsAsync(string imageUri)
    {
        try
        {
            StorageFile file;

            // Check if it's a URI or a file system path
            if (imageUri.Contains("://"))
            {
                // Handle URIs (ms-appx://, ms-appdata://)
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(imageUri));
            }
            else if (System.IO.Path.IsPathRooted(imageUri))
            {
                // Handle absolute file paths
                file = await StorageFile.GetFileFromPathAsync(imageUri);
            }
            else
            {
                // Handle relative paths by converting to ms-appx:// URI
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{imageUri}"));
            }

            var properties = await file.Properties.GetImagePropertiesAsync();
            return (properties.Width, properties.Height);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get image dimensions from URI: {imageUri}", ex);
        }
    }
}
