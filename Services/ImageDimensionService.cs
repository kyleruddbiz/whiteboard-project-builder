using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for retrieving image dimensions without loading the full image into memory.
/// </summary>
public class ImageDimensionService
{
    /// <summary>
    /// Gets image dimensions from a StorageFile by reading file metadata only.
    /// </summary>
    /// <param name="file">StorageFile to get dimensions from</param>
    /// <returns>Tuple of (width, height) in pixels</returns>
    public async Task<(uint width, uint height)> GetImageDimensionsAsync(StorageFile file)
    {
        try
        {
            var properties = await file.Properties.GetImagePropertiesAsync();
            return (properties.Width, properties.Height);
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException($"Image file not found: {file.Path}");
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Access denied to image file: {file.Path}. The app may not have permission to read this file.");
        }
        catch (System.Runtime.InteropServices.COMException ex) when (ex.HResult == unchecked((int)0x80270003))
        {
            throw new InvalidOperationException($"The image file appears to be corrupted or in an unsupported format (WIC codec error).\n\nFile: {file.Name}\nPath: {file.Path}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get image dimensions from file: {file.Path}. Error: {ex.GetType().Name} - {ex.Message}", ex);
        }
    }

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
            else if (Path.IsPathRooted(imageUri))
            {
                // Handle absolute file paths
                file = await StorageFile.GetFileFromPathAsync(imageUri);
            }
            else
            {
                // Handle relative paths by converting to ms-appx:// URI
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{imageUri}"));
            }

            return await GetImageDimensionsAsync(file);
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException($"Image file not found at URI: {imageUri}");
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Access denied to image at URI: {imageUri}. The app may not have permission to read this location.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to get image dimensions from URI: {imageUri}. Error: {ex.GetType().Name} - {ex.Message}", ex);
        }
    }
}
