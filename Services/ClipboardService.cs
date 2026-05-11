using Windows.ApplicationModel.DataTransfer;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages clipboard operations.
/// </summary>
public class ClipboardService(ImageStorageService imageStorageService)
{
    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">Text to copy</param>
    public void CopyTextToClipboard(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Copies the images folder path to the clipboard.
    /// </summary>
    public async Task CopyImagesFolderPathToClipboardAsync()
    {
        string folderPath = await imageStorageService.GetImagesFolderPathAsync();
        CopyTextToClipboard(folderPath);
    }
}
