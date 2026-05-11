using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Constants;
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
        if (SelectedItem != null && SelectedItem != item)
        {
            SelectedItem.IsEditing = false;
        }

        SelectedItem = item;
        item.IsEditing = true;
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

    public async Task ReplaceImageAsync(WhiteboardItemViewModelBase item, XamlRoot xamlRoot)
    {
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

            string? title = null;
            string? subtitle = null;

            if (item is ProjectItemViewModel project)
            {
                title = project.Title;
                subtitle = project.Subtitle;
            }
            else if (item is TaskItemViewModel task)
            {
                title = task.Title;
                subtitle = task.Subtitle;
            }

            string fileName = imageStorageService.GenerateFileName(title, subtitle);
            string imageUri = await imageStorageService.SaveImageAsync(file.Path, fileName + Path.GetExtension(file.Path));

            if (item is ProjectItemViewModel projectItem)
            {
                var (scale, offsetX, offsetY) = imageTransformService.CalculateDefaultTransform(
                    width, height,
                    ImageLayoutConstants.Project.ClipWidth,
                    ImageLayoutConstants.Project.ClipHeight);
                projectItem.ImageZoomFactor = scale;
                projectItem.ImageOffsetX = offsetX;
                projectItem.ImageOffsetY = offsetY;
                projectItem.Image = imageUri;
            }
            else if (item is TaskItemViewModel taskItem)
            {
                var (scale, offsetX, offsetY) = imageTransformService.CalculateDefaultTransform(
                    width, height,
                    ImageLayoutConstants.Task.ClipWidth,
                    ImageLayoutConstants.Task.ClipHeight);
                taskItem.ImageZoomFactor = scale;
                taskItem.ImageOffsetX = offsetX;
                taskItem.ImageOffsetY = offsetY;
                taskItem.Image = imageUri;
            }

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
