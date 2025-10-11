using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;
    private readonly DataPersistenceService dataPersistenceService;
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;
    private CancellationTokenSource? saveCts;

    private const int AutoSaveDelayMs = 2000;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private DateTime? lastSaved;

    [ObservableProperty]
    private ProjectItemViewModel? selectedProject;

    [ObservableProperty]
    private bool showArchived;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel(PrintService printService, DataPersistenceService dataPersistenceService)
    {
        this.printService = printService;
        this.dataPersistenceService = dataPersistenceService;
        this.imageStorageService = new ImageStorageService();
        this.imageTransformService = new ImageTransformService();
        this.imageDimensionService = new ImageDimensionService();

        Projects = [];
        GridItems = [];

        Projects.CollectionChanged += Projects_CollectionChanged;
        Projects.CollectionChanged += OnProjectsCollectionChanged;

        // Load projects from storage
        _ = LoadProjectsAsync();

        SampleGoal = new GoalItemViewModel
        {
            Title = "Draw Forms",
            Subtitle = "Practice Figure Drawing",
            Image = "Assets/Backgrounds/Examples/portrait-2.jpg"
        };

        SampleInspiration = new InspirationItemViewModel
        {
            Text = "Some text that really inspires the people and relates in a clever way to puzzles\n\n– That Famous Person",
            Image = "Assets/Backgrounds/Examples/landscape-3.jpg"
        };
    }

    [RelayCommand]
    private async Task PrintProjectsAsync()
    {
        await printService.ShowPrintUIAsync(Projects);
    }

    [RelayCommand]
    private async Task OpenImagesFolderAsync()
    {
        try
        {
            string folderPath = await ImageStorageService.GetImagesFolderPathAsync();
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open images folder: {ex.Message}");
        }
    }

    private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGridItems();
    }

    private void OnProjectsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to DataChanged event for new items
        if (e.NewItems != null)
        {
            foreach (ProjectItemViewModel item in e.NewItems)
            {
                item.DataChanged += OnProjectDataChanged;
            }
        }

        // Unsubscribe from removed items to prevent memory leaks
        if (e.OldItems != null)
        {
            foreach (ProjectItemViewModel item in e.OldItems)
            {
                item.DataChanged -= OnProjectDataChanged;
            }
        }

        // Trigger autosave on collection changes
        _ = TriggerAutoSaveAsync();
    }

    private void OnProjectDataChanged(object? sender, EventArgs e)
    {
        _ = TriggerAutoSaveAsync();
    }

    private void RebuildGridItems()
    {
        GridItems.Clear();

        foreach (var project in Projects)
        {
            // Filter based on ShowArchived setting
            if (ShowArchived || !project.IsArchived)
            {
                GridItems.Add(new GridItemWrapper { ProjectItem = project });
            }
        }

        GridItems.Add(new GridItemWrapper { IsAddButton = true });
    }

    /// <summary>
    /// Debounced autosave - waits for user to stop making changes before saving.
    /// </summary>
    private async Task TriggerAutoSaveAsync()
    {
        // Cancel any pending save operation
        saveCts?.Cancel();
        saveCts = new CancellationTokenSource();

        try
        {
            // Wait for delay period - resets on each change
            await Task.Delay(AutoSaveDelayMs, saveCts.Token);

            // Delay completed without cancellation - perform save
            await SaveDataAsync();
        }
        catch (TaskCanceledException)
        {
            // Expected when debouncing - do nothing
        }
    }

    /// <summary>
    /// Saves projects to JSON file.
    /// </summary>
    private async Task SaveDataAsync()
    {
        var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        dispatcherQueue.TryEnqueue(() => IsSaving = true);

        try
        {
            await dataPersistenceService.SaveProjectsAsync(Projects);

            dispatcherQueue.TryEnqueue(() =>
            {
                LastSaved = DateTime.Now;
                IsSaving = false;
            });
        }
        catch (Exception ex)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                IsSaving = false;
                // TODO: Show error message to user
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
            });
        }
    }

    /// <summary>
    /// Loads projects from JSON file on startup.
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        try
        {
            var loadedProjects = await dataPersistenceService.LoadProjectsAsync();

            foreach (var project in loadedProjects)
            {
                Projects.Add(project);
            }

            RebuildGridItems();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Load failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Forces immediate save, bypassing debounce. Used on app suspend.
    /// </summary>
    public async Task ForceSaveAsync()
    {
        saveCts?.Cancel();
        await SaveDataAsync();
    }

    [RelayCommand]
    private async Task AddProject()
    {
        ExitEditMode();

        var random = new Random();

        string imageType = (random.NextSingle() < .5f)
            ? "landscape"
            : "portrait";
        int imageNumber = random.Next(1, 6);

        string imagePath = $"Assets/Backgrounds/Examples/{imageType}-{imageNumber}.jpg";

        var newProject = new ProjectItemViewModel
        {
            Image = imagePath,
            Size = ProjectSize.Medium,
            Value = ProjectValue.Good,
            DueDate = null
        };

        // Apply UniformToFill transform for default background
        await ApplyUniformToFillTransformAsync(newProject, imagePath);

        Projects.Add(newProject);
        EnterEditMode(newProject);
    }

    [RelayCommand]
    private void ToggleShowArchived()
    {
        ShowArchived = !ShowArchived;
        RebuildGridItems();
    }

    [RelayCommand]
    private void RemoveProject(ProjectItemViewModel project)
    {
        if (SelectedProject == project)
        {
            ExitEditMode();
        }

        // If already archived, permanently delete
        // If not archived, archive it
        if (project.IsArchived)
        {
            Projects.Remove(project);
        }
        else
        {
            project.IsArchived = true;
            RebuildGridItems();
        }
    }

    [RelayCommand]
    private void ReactivateProject(ProjectItemViewModel project)
    {
        project.IsArchived = false;
        RebuildGridItems();
        ExitEditMode();
    }

    [RelayCommand]
    private void EnterEditMode(ProjectItemViewModel item)
    {
        if (SelectedProject != null && SelectedProject != item)
        {
            SelectedProject.IsEditing = false;
        }

        SelectedProject = item;
        item.IsEditing = true;
    }

    [RelayCommand]
    private void ExitEditMode()
    {
        if (SelectedProject != null)
        {
            SelectedProject.IsEditing = false;
            SelectedProject = null;
        }
    }

    public async Task PasteImageFromClipboardAsync(Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        try
        {
            // Generate default filename
            string defaultFileName = imageStorageService.GenerateFileName(
                SelectedProject?.Title,
                SelectedProject?.Subtitle
            );

            // Show save dialog
            var dialog = new Views.SaveImageDialog(defaultFileName)
            {
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            // If user cancelled, exit
            if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            // Validate filename
            if (string.IsNullOrWhiteSpace(dialog.FileName))
            {
                await ShowErrorDialogAsync(xamlRoot, "Invalid Filename", "Please enter a valid filename.");
                return;
            }

            // Save the image from clipboard (returns URI and dimensions)
            var (imageUri, width, height) = await imageStorageService.SaveBitmapFromClipboardAsync(dialog.FileName);

            // Update selected project if one exists
            if (SelectedProject != null)
            {
                // Calculate and apply UniformToFill transform
                var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

                SelectedProject.ImageScale = scale;
                SelectedProject.ImageOffsetX = offsetX;
                SelectedProject.ImageOffsetY = offsetY;
                SelectedProject.Image = imageUri;
            }

            // Show success notification (optional)
            System.Diagnostics.Debug.WriteLine($"Image saved successfully: {imageUri}");
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

    public async Task ReplaceImageAsync(Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        if (SelectedProject == null)
        {
            return;
        }

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            // Get dimensions from the original file path
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(file.Path);

            // Generate filename from project details
            string fileName = imageStorageService.GenerateFileName(
                SelectedProject.Title,
                SelectedProject.Subtitle
            );

            // Save the image
            var imageUri = await imageStorageService.SaveImageAsync(file.Path, fileName + System.IO.Path.GetExtension(file.Path));

            // Calculate and apply UniformToFill transform
            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            SelectedProject.ImageScale = scale;
            SelectedProject.ImageOffsetX = offsetX;
            SelectedProject.ImageOffsetY = offsetY;
            SelectedProject.Image = imageUri;

            System.Diagnostics.Debug.WriteLine($"Image replaced successfully: {imageUri}");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Error Replacing Image", $"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task ShowErrorDialogAsync(Microsoft.UI.Xaml.XamlRoot xamlRoot, string title, string message)
    {
        var errorDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await errorDialog.ShowAsync();
    }

    /// <summary>
    /// Applies UniformToFill transform to a project item based on its image URI.
    /// Used for default background images loaded from ms-appx:// URIs.
    /// </summary>
    private async Task ApplyUniformToFillTransformAsync(ProjectItemViewModel project, string imageUri)
    {
        try
        {
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(imageUri);
            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            project.ImageScale = scale;
            project.ImageOffsetX = offsetX;
            project.ImageOffsetY = offsetY;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply UniformToFill transform: {ex.Message}");
            // Leave default values (scale=1.0, offset=0,0)
        }
    }
}