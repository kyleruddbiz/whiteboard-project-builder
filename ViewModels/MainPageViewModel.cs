using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Windows.Storage.Pickers;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;
    private readonly DataPersistenceService dataPersistenceService;
    private readonly SettingsService settingsService;
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;
    private CancellationTokenSource? saveCts;
    private CancellationTokenSource? settingsSaveCts;

    private const int AutoSaveDelayMs = 2000;
    private readonly List<string> exampleImagePaths = [];

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private DateTime? lastSaved;

    [ObservableProperty]
    private ProjectItemViewModel? selectedProject;

    [ObservableProperty]
    private SettingsViewModel settings = null!;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }

    public MainPageViewModel(PrintService printService, DataPersistenceService dataPersistenceService, SettingsService settingsService)
    {
        this.printService = printService;
        this.dataPersistenceService = dataPersistenceService;
        this.settingsService = settingsService;
        imageStorageService = new ImageStorageService();
        imageTransformService = new ImageTransformService();
        imageDimensionService = new ImageDimensionService();

        Settings = new SettingsViewModel();

        Projects = [];
        GridItems = [];

        GridItems.Add(new GridItemWrapper { IsAddButton = true });

        Projects.CollectionChanged += Projects_CollectionChanged;
        Projects.CollectionChanged += OnProjectsCollectionChanged;

        _ = LoadDataAsync();
    }

    partial void OnSettingsChanged(SettingsViewModel? oldValue, SettingsViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnSettingsDataChanged;
            oldValue.PropertyChanged -= OnSettingsPropertyChanged;
        }

        if (newValue != null)
        {
            newValue.DataChanged += OnSettingsDataChanged;
            newValue.PropertyChanged += OnSettingsPropertyChanged;
        }
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.IsDeveloperMode))
        {
            LoadExampleImagePaths();
        }
    }

    [RelayCommand]
    private async Task PrintProjectsAsync()
    {
        var activeProjects = Projects.Where(p => !p.IsArchived);
        await printService.ShowPrintUIAsync(activeProjects);
    }

    [RelayCommand]
    private async Task OpenImagesFolderAsync()
    {
        try
        {
            string folderPath = await ImageStorageService.GetImagesFolderPathAsync();
            Process.Start("explorer.exe", folderPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open images folder: {ex.Message}");
        }
    }

    private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (ProjectItemViewModel item in e.NewItems)
                    {
                        AddGridItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (ProjectItemViewModel item in e.OldItems)
                    {
                        RemoveGridItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                RebuildGridItems();
                break;

            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                RebuildGridItems();
                break;
        }
    }

    private void OnProjectsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ProjectItemViewModel item in e.NewItems)
            {
                item.DataChanged += OnProjectDataChanged;
                item.PropertyChanged += OnProjectPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ProjectItemViewModel item in e.OldItems)
            {
                item.DataChanged -= OnProjectDataChanged;
                item.PropertyChanged -= OnProjectPropertyChanged;
            }
        }

        _ = TriggerAutoSaveAsync();
    }

    private void OnProjectDataChanged(object? sender, EventArgs e)
    {
        _ = TriggerAutoSaveAsync();
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectItemViewModel.IsArchived) && sender is ProjectItemViewModel project)
        {
            if (!Settings.ShowArchived)
            {
                if (project.IsArchived)
                {
                    RemoveGridItem(project);
                }
                else
                {
                    AddGridItem(project);
                }
            }
        }
    }

    private void AddGridItem(ProjectItemViewModel project)
    {
        if (!Settings.ShowArchived && project.IsArchived)
        {
            return;
        }

        if (GridItems.Any(w => w.ProjectItem == project))
        {
            return;
        }

        int insertIndex;

        if (Settings.IsSortDescending)
        {
            insertIndex = GridItems.Count > 0 && GridItems[0].IsAddButton ? 1 : 0;
        }
        else
        {
            insertIndex = GridItems.Count > 0 && GridItems[^1].IsAddButton
                ? GridItems.Count - 1
                : GridItems.Count;
        }

        GridItems.Insert(insertIndex, new GridItemWrapper { ProjectItem = project });
    }

    private void RemoveGridItem(ProjectItemViewModel project)
    {
        var wrapper = GridItems.FirstOrDefault(w => w.ProjectItem == project);
        if (wrapper != null)
        {
            GridItems.Remove(wrapper);
        }
    }

    private void RebuildGridItems()
    {
        GridItems.Clear();

        if (Settings.IsSortDescending)
        {
            GridItems.Add(new GridItemWrapper { IsAddButton = true });
        }

        var projectsToDisplay = Projects.Where(p => Settings.ShowArchived || !p.IsArchived);

        var sortedProjects = Settings.IsSortDescending
            ? projectsToDisplay.Reverse()
            : projectsToDisplay;

        foreach (var project in sortedProjects)
        {
            GridItems.Add(new GridItemWrapper { ProjectItem = project });
        }

        if (!Settings.IsSortDescending)
        {
            GridItems.Add(new GridItemWrapper { IsAddButton = true });
        }
    }

    /// <summary>
    /// Debounced autosave - waits for user to stop making changes before saving.
    /// </summary>
    private async Task TriggerAutoSaveAsync()
    {
        saveCts?.Cancel();
        saveCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(AutoSaveDelayMs, saveCts.Token);
            await SaveDataAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }

    /// <summary>
    /// Saves projects to JSON file.
    /// </summary>
    private async Task SaveDataAsync()
    {
        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

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
                Debug.WriteLine($"Save failed: {ex.Message}");
            });
        }
    }

    /// <summary>
    /// Loads projects and settings from JSON files on startup.
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            var loadedSettings = await settingsService.LoadSettingsAsync();
            Settings = SettingsViewModel.FromModel(loadedSettings);

            LoadExampleImagePaths();

            var loadedProjects = await dataPersistenceService.LoadProjectsAsync();

            foreach (var project in loadedProjects)
            {
                Projects.Add(project);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Load failed: {ex.Message}");
        }
    }

    private void OnSettingsDataChanged(object? sender, EventArgs e)
    {
        _ = TriggerSettingsAutoSaveAsync();
    }

    /// <summary>
    /// Debounced autosave for settings - waits for user to stop making changes before saving.
    /// </summary>
    private async Task TriggerSettingsAutoSaveAsync()
    {
        settingsSaveCts?.Cancel();
        settingsSaveCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(AutoSaveDelayMs, settingsSaveCts.Token);
            await SaveSettingsAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }

    /// <summary>
    /// Saves settings to JSON file.
    /// </summary>
    private async Task SaveSettingsAsync()
    {
        try
        {
            await settingsService.SaveSettingsAsync(Settings.ToModel());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Settings save failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Forces immediate save, bypassing debounce. Used on app suspend.
    /// </summary>
    public async Task ForceSaveAsync()
    {
        saveCts?.Cancel();
        settingsSaveCts?.Cancel();
        await Task.WhenAll(SaveDataAsync(), SaveSettingsAsync());
    }

    [RelayCommand]
    private async Task AddProjectAsync()
    {
        ExitEditMode();

        if (exampleImagePaths.Count == 0)
        {
            throw new InvalidOperationException("No example images available. Failed to load images from Assets/Backgrounds/Examples directory.");
        }

        var random = new Random();
        int randomIndex = random.Next(exampleImagePaths.Count);
        string imagePath = exampleImagePaths[randomIndex];

        var newProject = new ProjectItemViewModel
        {
            Image = imagePath,
            Size = ProjectSize.Medium,
            Value = ProjectValue.Good,
            DueDate = null
        };

        await ApplyUniformToFillTransformAsync(newProject, imagePath);

        Projects.Add(newProject);
        EnterEditMode(newProject);
    }

    [RelayCommand]
    private void ToggleShowArchived()
    {
        Settings.ShowArchived = !Settings.ShowArchived;
        RebuildGridItems();
    }

    [RelayCommand]
    private void ToggleSortOrder()
    {
        Settings.IsSortDescending = !Settings.IsSortDescending;
        RebuildGridItems();
    }

    [RelayCommand]
    private void RemoveProject(ProjectItemViewModel project)
    {
        if (SelectedProject == project)
        {
            ExitEditMode();
        }

        if (project.IsArchived)
        {
            Projects.Remove(project);
        }
        else
        {
            project.IsArchived = true;
        }
    }

    [RelayCommand]
    private void ReactivateProject(ProjectItemViewModel project)
    {
        project.IsArchived = false;
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

    public async Task PasteImageFromClipboardAsync(XamlRoot xamlRoot)
    {
        try
        {
            string defaultFileName = imageStorageService.GenerateFileName(
                SelectedProject?.Title,
                SelectedProject?.Subtitle
            );

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

            if (SelectedProject != null)
            {
                var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

                SelectedProject.ImageZoomFactor = scale;
                SelectedProject.ImageOffsetX = offsetX;
                SelectedProject.ImageOffsetY = offsetY;
                SelectedProject.Image = imageUri;
            }

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

    public async Task ReplaceImageAsync(XamlRoot xamlRoot)
    {
        if (SelectedProject == null)
        {
            return;
        }

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

            nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(file.Path);

            string fileName = imageStorageService.GenerateFileName(
                SelectedProject.Title,
                SelectedProject.Subtitle
            );

            string imageUri = await imageStorageService.SaveImageAsync(file.Path, fileName + Path.GetExtension(file.Path));

            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            SelectedProject.ImageZoomFactor = scale;
            SelectedProject.ImageOffsetX = offsetX;
            SelectedProject.ImageOffsetY = offsetY;
            SelectedProject.Image = imageUri;

            Debug.WriteLine($"Image replaced successfully: {imageUri}");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync(xamlRoot, "Error Replacing Image", $"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task ShowErrorDialogAsync(XamlRoot xamlRoot, string title, string message)
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

            project.ImageZoomFactor = scale;
            project.ImageOffsetX = offsetX;
            project.ImageOffsetY = offsetY;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply UniformToFill transform: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads all example background image paths from the Assets/Backgrounds/Examples or ExamplesAlt directory.
    /// </summary>
    private void LoadExampleImagePaths()
    {
        try
        {
            string folderName = Settings.IsDeveloperMode ? "ExamplesAlt" : "Examples";
            string examplesPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Backgrounds", folderName);

            exampleImagePaths.Clear();

            if (Directory.Exists(examplesPath))
            {
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

                foreach (string filePath in Directory.GetFiles(examplesPath))
                {
                    if (imageExtensions.Contains(Path.GetExtension(filePath)))
                    {
                        string relativePath = $"Assets/Backgrounds/{folderName}/{Path.GetFileName(filePath)}";
                        exampleImagePaths.Add(relativePath);
                    }
                }
            }

            if (exampleImagePaths.Count == 0)
            {
                Debug.WriteLine($"Warning: No example images found in Assets/Backgrounds/{folderName}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load example image paths: {ex.Message}");
        }
    }
}
