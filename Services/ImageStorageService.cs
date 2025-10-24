using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages storage of user-selected images in LocalFolder and provides default example images.
/// </summary>
public class ImageStorageService
{
    private const string ImagesFolderName = "Images";
    private readonly SettingsService settingsService;
    private List<string>? exampleImagePaths;

    public ImageStorageService(SettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    /// <summary>
    /// Ensures the images folder exists in LocalFolder.
    /// </summary>
    public static async Task EnsureImagesFolderExistsAsync()
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

        await EnsureImagesFolderExistsAsync();
        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);

        // Ensure .png extension
        string sanitizedFileName = SanitizeFileName(fileName);
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

        // Try to get PNG format first (preserves transparency better)
        if (dataPackageView.AvailableFormats.Contains("PNG"))
        {
            if (await dataPackageView.GetDataAsync("PNG") is IRandomAccessStream pngData)
            {
                using (var fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var decoder = await BitmapDecoder.CreateAsync(pngData);
                    width = (int)decoder.PixelWidth;
                    height = (int)decoder.PixelHeight;

                    // Copy PNG data directly to preserve transparency
                    pngData.Seek(0);
                    await RandomAccessStream.CopyAsync(pngData, fileStream);
                    await fileStream.FlushAsync();
                }

                return ($"ms-appdata:///local/{ImagesFolderName}/{storageFile.Name}", width, height);
            }
        }

        // Fallback to standard bitmap format
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

        using (var imageStream = await imageReference.OpenReadAsync())
        using (var fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);

            // Capture dimensions before decoder goes out of scope
            width = (int)decoder.PixelWidth;
            height = (int)decoder.PixelHeight;

            // Encode as PNG
            var encoder = await BitmapEncoder.CreateAsync(
                BitmapEncoder.PngEncoderId,
                fileStream
            );

            // Request pixel data with premultiplied alpha to preserve transparency
            var pixelDataProvider = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage
            );
            byte[] pixelData = pixelDataProvider.DetachPixelData();

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
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

        string baseFileName = string.Join("-", parts);
        return SanitizeFileName(baseFileName);
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters.
    /// </summary>
    /// <param name="fileName">Original filename</param>
    /// <returns>Sanitized filename safe for file system</returns>
    private string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = new([.. fileName.Where(ch => !invalidChars.Contains(ch))]);

        // Replace multiple spaces/dashes with single dash
        sanitized = Regex.Replace(sanitized, @"[\s\-]+", "-");

        sanitized = sanitized.Trim('-');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = $"clipboard-image-{DateTime.Now:yyyy-MM-dd-HHmmss}";
        }

        return sanitized;
    }

    /// <summary>
    /// Gets the full path to the images folder in LocalFolder.
    /// </summary>
    public static async Task<string> GetImagesFolderPathAsync()
    {
        await EnsureImagesFolderExistsAsync();
        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);
        return imagesFolder.Path;
    }

    /// <summary>
    /// Gets a random default image path from the example images collection.
    /// </summary>
    /// <returns>Relative path to a random example image</returns>
    /// <exception cref="InvalidOperationException">Thrown when no example images are available</exception>
    public async Task<string> GetRandomDefaultImagePathAsync()
    {
        exampleImagePaths ??= await LoadExampleImagePathsAsync();

        if (exampleImagePaths.Count == 0)
        {
            throw new InvalidOperationException("No example images available for default image selection.");
        }

        var random = new Random();
        int randomIndex = random.Next(exampleImagePaths.Count);
        return exampleImagePaths[randomIndex];
    }

    /// <summary>
    /// Reloads example image paths from disk. Call this when developer mode setting changes.
    /// </summary>
    public void ReloadExampleImagePaths()
    {
        exampleImagePaths = null;
    }

    /// <summary>
    /// Loads example image paths from the appropriate folder based on developer mode setting.
    /// </summary>
    private async Task<List<string>> LoadExampleImagePathsAsync()
    {
        var paths = new List<string>();

        try
        {
            var settings = await settingsService.LoadSettingsAsync();
            string folderName = settings.IsDeveloperMode ? "ExamplesAlt" : "Examples";
            string examplesPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Backgrounds", folderName);

            if (Directory.Exists(examplesPath))
            {
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

                foreach (string filePath in Directory.GetFiles(examplesPath))
                {
                    if (imageExtensions.Contains(Path.GetExtension(filePath)))
                    {
                        string relativePath = $"Assets/Backgrounds/{folderName}/{Path.GetFileName(filePath)}";
                        paths.Add(relativePath);
                    }
                }
            }

            if (paths.Count == 0)
            {
                Debug.WriteLine($"Warning: No example images found in Assets/Backgrounds/{folderName}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load example image paths: {ex.Message}");
        }

        return paths;
    }
}
