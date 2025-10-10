using Windows.ApplicationModel.DataTransfer;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Manages clipboard operations.
/// </summary>
public static class ClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">Text to copy</param>
    public static void CopyTextToClipboard(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Copies the images folder path to the clipboard.
    /// </summary>
    public static async Task CopyImagesFolderPathToClipboardAsync()
    {
        string folderPath = await ImageStorageService.GetImagesFolderPathAsync();
        CopyTextToClipboard(folderPath);
    }
}
