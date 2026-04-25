using Windows.Graphics.Imaging;
using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for retrieving image dimensions without loading the full image into memory.
/// </summary>
/// <remarks>
/// Uses <see cref="BitmapDecoder"/> rather than <c>StorageFile.Properties.GetImagePropertiesAsync()</c>:
/// the latter relies on the Windows property store / indexer and silently returns zero dimensions
/// for packaged-asset (<c>ms-appx://</c>) files and other unindexed locations. <c>BitmapDecoder</c>
/// reads dimensions directly from the file content via WIC (the same path the XAML <c>Image</c>
/// control uses), and <c>OrientedPixelWidth/Height</c> gives the post-EXIF-rotation dimensions
/// that match what's actually displayed.
/// </remarks>
public class ImageDimensionService
{
    public async Task<(uint width, uint height)> GetImageDimensionsAsync(StorageFile file)
    {
        try
        {
            using var stream = await file.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            uint width = decoder.OrientedPixelWidth;
            uint height = decoder.OrientedPixelHeight;
            if (width == 0 || height == 0)
            {
                throw new InvalidOperationException($"Image decoded with zero dimensions: {file.Path}");
            }
            return (width, height);
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException($"Image file not found: {file.Path}");
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Access denied to image file: {file.Path}.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to decode image: {file.Path}. {ex.GetType().Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets image dimensions from a URI or file path. Supports <c>ms-appx://</c>,
    /// <c>ms-appdata://</c>, and absolute or relative file paths.
    /// </summary>
    public async Task<(uint width, uint height)> GetImageDimensionsAsync(string imageUri)
    {
        StorageFile file;
        if (imageUri.Contains("://"))
        {
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(imageUri));
        }
        else if (Path.IsPathRooted(imageUri))
        {
            file = await StorageFile.GetFileFromPathAsync(imageUri);
        }
        else
        {
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{imageUri}"));
        }
        return await GetImageDimensionsAsync(file);
    }
}
