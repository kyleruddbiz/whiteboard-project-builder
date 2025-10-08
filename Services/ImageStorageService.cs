using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages storage of user-selected images in LocalFolder.
/// </summary>
public class ImageStorageService
{
    private const string ImagesFolderName = "Images";

    /// <summary>
    /// Ensures the images folder exists in LocalFolder.
    /// </summary>
    public async Task EnsureImagesFolderExistsAsync()
    {
        await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            ImagesFolderName,
            CreationCollisionOption.OpenIfExists
        );
    }

    /// <summary>
    /// Saves an image to LocalFolder and returns the ms-appdata:/// URI.
    /// </summary>
    /// <param name="sourcePath">Path to source image file</param>
    /// <param name="fileName">Desired file name for the saved image</param>
    /// <returns>URI path for the saved image (ms-appdata:///local/Images/fileName)</returns>
    public async Task<string> SaveImageAsync(string sourcePath, string fileName)
    {
        await EnsureImagesFolderExistsAsync();

        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);
        var sourceFile = await StorageFile.GetFileFromPathAsync(sourcePath);
        await sourceFile.CopyAsync(imagesFolder, fileName, NameCollisionOption.ReplaceExisting);

        return $"ms-appdata:///local/{ImagesFolderName}/{fileName}";
    }

    /// <summary>
    /// Saves a bitmap from the clipboard to LocalFolder and returns the ms-appdata:/// URI with dimensions.
    /// </summary>
    /// <param name="fileName">Desired file name for the saved image (without extension)</param>
    /// <returns>Tuple of (uri, width, height) for the saved image</returns>
    public async Task<(string uri, int width, int height)> SaveBitmapFromClipboardAsync(string fileName)
    {
        var dataPackageView = Clipboard.GetContent();

        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
        {
            throw new InvalidOperationException("Clipboard does not contain a bitmap image.");
        }

        IRandomAccessStreamReference? imageReference = null;
        try
        {
            imageReference = await dataPackageView.GetBitmapAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve bitmap from clipboard: {ex.Message}", ex);
        }

        if (imageReference == null)
        {
            throw new InvalidOperationException("Failed to retrieve bitmap from clipboard.");
        }

        await EnsureImagesFolderExistsAsync();

        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);

        // Ensure .png extension
        var sanitizedFileName = SanitizeFileName(fileName);
        if (!sanitizedFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            sanitizedFileName += ".png";
        }

        var storageFile = await imagesFolder.CreateFileAsync(
            sanitizedFileName,
            CreationCollisionOption.GenerateUniqueName
        );

        int width;
        int height;

        using (var imageStream = await imageReference.OpenReadAsync())
        using (var fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
        {
            // Decode the bitmap
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);

            // Capture dimensions before decoder goes out of scope
            width = (int)decoder.PixelWidth;
            height = (int)decoder.PixelHeight;

            // Encode as PNG
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(
                BitmapEncoder.PngEncoderId,
                fileStream
            );

            var pixelDataProvider = await decoder.GetPixelDataAsync();
            var pixelData = pixelDataProvider.DetachPixelData();

            encoder.SetPixelData(
                decoder.BitmapPixelFormat,
                decoder.BitmapAlphaMode,
                decoder.PixelWidth,
                decoder.PixelHeight,
                decoder.DpiX,
                decoder.DpiY,
                pixelData
            );

            await encoder.FlushAsync();
        }

        return ($"ms-appdata:///local/{ImagesFolderName}/{storageFile.Name}", width, height);
    }

    /// <summary>
    /// Generates a unique filename from project title and subtitle.
    /// </summary>
    /// <param name="title">Project title</param>
    /// <param name="subtitle">Project subtitle (optional)</param>
    /// <returns>Sanitized filename without extension</returns>
    public string GenerateFileName(string? title, string? subtitle)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(title))
        {
            parts.Add(title.Trim());
        }

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            parts.Add(subtitle.Trim());
        }

        if (parts.Count == 0)
        {
            return $"clipboard-image-{DateTime.Now:yyyy-MM-dd-HHmmss}";
        }

        var baseFileName = string.Join("-", parts);
        return SanitizeFileName(baseFileName);
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters.
    /// </summary>
    /// <param name="fileName">Original filename</param>
    /// <returns>Sanitized filename safe for file system</returns>
    private string SanitizeFileName(string fileName)
    {
        // Remove invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());

        // Replace multiple spaces/dashes with single dash
        sanitized = Regex.Replace(sanitized, @"[\s\-]+", "-");

        // Remove leading/trailing dashes
        sanitized = sanitized.Trim('-');

        // If empty after sanitization, use timestamp
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = $"clipboard-image-{DateTime.Now:yyyy-MM-dd-HHmmss}";
        }

        return sanitized;
    }

    /// <summary>
    /// Gets the full path to the images folder in LocalFolder.
    /// </summary>
    public async Task<string> GetImagesFolderPathAsync()
    {
        await EnsureImagesFolderExistsAsync();
        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);
        return imagesFolder.Path;
    }
}
