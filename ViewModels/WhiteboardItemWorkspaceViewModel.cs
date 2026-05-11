using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class WhiteboardItemWorkspaceViewModel(
    ImageStorageService imageStorageService,
    ImageTransformService imageTransformService,
    ImageDimensionService imageDimensionService) : ObservableObject
{
    [ObservableProperty]
    private WhiteboardItemViewModelBase? selectedItem;

    public void EnterEditMode(WhiteboardItemViewModelBase item)
    {
        if (SelectedItem == item) return;

        SelectedItem?.IsEditing = false;
        SelectedItem = item;
        SelectedItem.IsEditing = true;
    }

    public void ExitEditMode()
    {
        SelectedItem?.IsEditing = false;
        SelectedItem = null;
    }

    public void ReactivateItem(WhiteboardItemViewModelBase item)
    {
        item.IsArchived = false;

        ExitEditMode();
    }

    public void ApplyImageWithDefaultTransform(ISingleImageItem item, string imageUri, double width, double height)
    {
        var (scale, offsetX, offsetY) = imageTransformService.CalculateDefaultTransform(
            width, height, item.ImageClipWidth, item.ImageClipHeight);

        item.ImageZoomFactor = scale;
        item.ImageOffsetX = offsetX;
        item.ImageOffsetY = offsetY;
        item.Image = imageUri;
    }

    public async Task ReplaceImageAsync(WhiteboardItemViewModelBase item, XamlRoot xamlRoot)
    {
        if (item is not ISingleImageItem imageItem)
        {
            return;
        }

        StorageFile? file = null;

        try
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            nint hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(file);

            var titledItem = item as ITitledItem;
            string fileName = imageStorageService.GenerateFileName(titledItem?.Title, titledItem?.Subtitle);
            string imageUri = await imageStorageService.SaveImageAsync(file.Path, fileName + Path.GetExtension(file.Path));

            ApplyImageWithDefaultTransform(imageItem, imageUri, width, height);

            Debug.WriteLine($"Image replaced successfully: {imageUri}");
        }
        catch (InvalidOperationException ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Image Processing Error", ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await ShowErrorDialogAsync(xamlRoot, "Access Denied", "The app does not have permission to access the selected file. Please try selecting a file from a different location, such as your Pictures folder.");
        }
        catch (FileNotFoundException ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "File Not Found", $"The selected image file could not be found:\n{ex.Message}");
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            string errorDetails = $"Type: {ex.GetType().Name}\nHResult: 0x{ex.HResult:X8} ({ex.HResult})\nMessage: {(string.IsNullOrEmpty(ex.Message) ? "(no message)" : ex.Message)}";

            string suggestion = ex.HResult switch
            {
                unchecked((int)0x80070005) => "\n\nThis is an access denied error. The file may be locked or you don't have permission to access it.",
                unchecked((int)0x80004005) => "\n\nThis is a general failure error. The file may be corrupted or in an unsupported format.",
                unchecked((int)0x800700B7) => "\n\nA file with this name already exists. Try deleting the old file from the Images folder first.",
                unchecked((int)0x80270003) => "\n\nThis is a WIC codec error (WINCODEC_ERR_COMPONENTNOTFOUND). The image file appears to be corrupted or uses an unsupported format.\n\nSuggestions:\n• Try opening the image in Paint and re-saving it\n• Convert the image to a standard PNG or JPG format\n• The file may be corrupted and need to be recreated",
                _ => $"\n\nFile path: {file?.Path ?? "unknown"}"
            };

            await ShowErrorDialogAsync(xamlRoot, "Error Replacing Image", errorDetails + suggestion);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Error Replacing Image", $"An unexpected error occurred:\n\nType: {ex.GetType().Name}\nMessage: {ex.Message}");
        }
    }

    public Task PasteImageAsync(XamlRoot xamlRoot)
    {
        if (SelectedItem == null)
        {
            return Task.CompletedTask;
        }

        return PasteImageAsync(SelectedItem, xamlRoot);
    }

    private async Task PasteImageAsync(WhiteboardItemViewModelBase item, XamlRoot xamlRoot)
    {
        if (item is not ISingleImageItem imageItem)
        {
            return;
        }

        try
        {
            var titledItem = item as ITitledItem;
            string defaultFileName = imageStorageService.GenerateFileName(titledItem?.Title, titledItem?.Subtitle);

            var dialog = new SaveImageDialog(defaultFileName)
            {
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dialog.FileName))
            {
                await ShowErrorDialogAsync(xamlRoot, "Invalid Filename", "Please enter a valid filename.");
                return;
            }

            var (imageUri, width, height) = await imageStorageService.SaveBitmapFromClipboardAsync(dialog.FileName);

            ApplyImageWithDefaultTransform(imageItem, imageUri, width, height);

            Debug.WriteLine($"Image saved successfully: {imageUri}");
        }
        catch (InvalidOperationException ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Clipboard Error", ex.Message);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Error Saving Image", $"An unexpected error occurred: {ex.Message}");
        }
    }

    private static async Task ShowErrorDialogAsync(XamlRoot xamlRoot, string title, string message)
    {
        var errorDialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await errorDialog.ShowAsync();
    }
}
