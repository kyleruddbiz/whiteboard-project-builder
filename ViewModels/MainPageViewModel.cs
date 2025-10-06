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
    private CancellationTokenSource? saveCts;

    private const int AutoSaveDelayMs = 2000;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private DateTime? lastSaved;

    public ObservableCollection<ProjectItemViewModel> Projects { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }
    public ProjectItemViewModel SampleProject { get; }
    public GoalItemViewModel SampleGoal { get; }
    public InspirationItemViewModel SampleInspiration { get; }

    public MainPageViewModel(PrintService printService, DataPersistenceService dataPersistenceService)
    {
        this.printService = printService;
        this.dataPersistenceService = dataPersistenceService;

        Projects = [];
        GridItems = [];

        SampleProject = new ProjectItemViewModel
        {
            Title = "Projects App",
            Subtitle = "Whiteboard Templates",
            Image = "Assets/Backgrounds/Examples/landscape-1.jpg",
            Size = ProjectSize.Large,
            Value = ProjectValue.Grand,
            DueDate = DateTime.Now.Date
        };

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
}