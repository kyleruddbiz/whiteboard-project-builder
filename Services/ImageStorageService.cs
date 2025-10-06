using Windows.Storage;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages storage of user-selected images in LocalFolder.
/// Future enhancement: will copy images to app data for custom backgrounds.
/// </summary>
public class ImageStorageService
{
    private const string ImagesFolderName = "images";

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
    /// Future implementation for user-selected custom backgrounds.
    /// </summary>
    /// <param name="sourcePath">Path to source image file</param>
    /// <param name="fileName">Desired file name for the saved image</param>
    /// <returns>URI path for the saved image (ms-appdata:///local/images/fileName)</returns>
    public async Task<string> SaveImageAsync(string sourcePath, string fileName)
    {
        await EnsureImagesFolderExistsAsync();

        var imagesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ImagesFolderName);
        var sourceFile = await StorageFile.GetFileFromPathAsync(sourcePath);
        await sourceFile.CopyAsync(imagesFolder, fileName, NameCollisionOption.ReplaceExisting);

        return $"ms-appdata:///local/{ImagesFolderName}/{fileName}";
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
