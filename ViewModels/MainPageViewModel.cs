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
    private CancellationTokenSource? saveCts;

    private const int AutoSaveDelayMs = 2000;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private DateTime? lastSaved;

    [ObservableProperty]
    private ProjectItemViewModel? selectedProject;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel(PrintService printService, DataPersistenceService dataPersistenceService)
    {
        this.printService = printService;
        this.dataPersistenceService = dataPersistenceService;
        this.imageStorageService = new ImageStorageService();

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
            GridItems.Add(new GridItemWrapper { ProjectItem = project });
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
    private void AddProject()
    {
        var random = new Random();

        string imageType = (random.NextSingle() < .5f)
            ? "landscape"
            : "portrait";
        int imageNumber = random.Next(1, 6);

        Projects.Add(new ProjectItemViewModel
        {
            Title = "New Project",
            Subtitle = "Add Details",
            Image = $"Assets/Backgrounds/Examples/{imageType}-{imageNumber}.jpg",
            Size = ProjectSize.Medium,
            Value = ProjectValue.Good,
            DueDate = null
        });
    }

    [RelayCommand]
    private void RemoveProject(ProjectItemViewModel project)
    {
        Projects.Remove(project);
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

            // Save the image from clipboard
            string imageUri = await imageStorageService.SaveBitmapFromClipboardAsync(dialog.FileName);

            // Update selected project if one exists
            if (SelectedProject != null)
            {
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
}