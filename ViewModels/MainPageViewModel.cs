using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;
using Windows.ApplicationModel.DataTransfer;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;
    private readonly WhiteboardItemRepository whiteboardItemRepository;
    private readonly SettingsService settingsService;
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;
    private readonly WhiteboardItemWorkspaceViewModel workspace;
    private readonly IServiceProvider serviceProvider;
    private CancellationTokenSource? saveCts;
    private CancellationTokenSource? settingsSaveCts;
    private bool archivedItemsLoaded;

    private const int AutoSaveDelayMs = 2000;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DateTime? lastSaved;

    [ObservableProperty]
    private SettingsViewModel settings = null!;

    public ObservableCollection<WhiteboardItemViewModelBase> WhiteboardItems { get; }

    public IReadOnlyList<WhiteboardSection> Sections { get; }

    public WhiteboardItemViewModelBase? SelectedItem
    {
        get => workspace.SelectedItem;
        set => workspace.SelectedItem = value;
    }

    public MainPageViewModel(
        PrintService printService,
        WhiteboardItemRepository whiteboardItemRepository,
        SettingsService settingsService,
        ImageStorageService imageStorageService,
        ImageTransformService imageTransformService,
        ImageDimensionService imageDimensionService,
        WhiteboardItemWorkspaceViewModel workspace,
        IServiceProvider serviceProvider)
    {
        this.printService = printService;
        this.whiteboardItemRepository = whiteboardItemRepository;
        this.settingsService = settingsService;
        this.imageStorageService = imageStorageService;
        this.imageTransformService = imageTransformService;
        this.imageDimensionService = imageDimensionService;
        this.workspace = workspace;
        this.serviceProvider = serviceProvider;

        Settings = serviceProvider.GetRequiredService<SettingsViewModel>();

        WhiteboardItems = [];
        Sections = ItemTypeSizeRegistry.ActiveSizes()
            .OrderByDescending(size => size)
            .Select(size => new WhiteboardSection(size))
            .ToList();

        RebuildGridItems();

        WhiteboardItems.CollectionChanged += WhiteboardItems_CollectionChanged;
        WhiteboardItems.CollectionChanged += OnWhiteboardItemsCollectionChanged;

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
            imageStorageService.ReloadExampleImagePaths();
        }
    }

    [RelayCommand]
    private async Task PrintWhiteboardItemsAsync()
    {
        var slots = BuildPrintSlots(WhiteboardItems.Where(i => !i.IsArchived));
        await printService.ShowPrintUIAsync(slots);
    }

    public static IEnumerable<IPrintSlot> BuildPrintSlots(IEnumerable<WhiteboardItemViewModelBase> items)
    {
        var itemList = items.ToList();

        foreach (var size in ItemTypeSizeRegistry.ActiveSizes().OrderByDescending(size => size))
        {
            var itemsAtSize = itemList.Where(i => i.LayoutSize == size);

            if (size == WhiteboardItemSize.Medium)
            {
                foreach (var item in itemsAtSize.OfType<IPrintSlot>())
                {
                    yield return item;
                }
            }
            else if (size == WhiteboardItemSize.Small)
            {
                foreach (var (top, bottom) in PairUpTasks(itemsAtSize.OfType<TaskItemViewModel>()))
                {
                    yield return new TaskSlotViewModel { TopTask = top, BottomTask = bottom };
                }
            }
        }
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

    [RelayCommand(CanExecute = nameof(CanPasteImage))]
    private async Task PasteImageAsync(XamlRoot xamlRoot)
    {
        await PasteImageFromClipboardAsync(xamlRoot);
    }

    private bool CanPasteImage(XamlRoot? xamlRoot)
    {
        var dataPackageView = Clipboard.GetContent();
        return dataPackageView.Contains(StandardDataFormats.Bitmap);
    }

    private void WhiteboardItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGridItems();
    }

    private void OnWhiteboardItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (WhiteboardItemViewModelBase item in e.NewItems)
            {
                item.DataChanged += OnWhiteboardItemDataChanged;
                item.PropertyChanged += OnWhiteboardItemPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (WhiteboardItemViewModelBase item in e.OldItems)
            {
                item.DataChanged -= OnWhiteboardItemDataChanged;
                item.PropertyChanged -= OnWhiteboardItemPropertyChanged;
            }
        }

        _ = TriggerAutoSaveAsync();
    }

    private void OnWhiteboardItemDataChanged(object? sender, EventArgs e)
    {
        _ = TriggerAutoSaveAsync();
    }

    private void OnWhiteboardItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WhiteboardItemViewModelBase.IsArchived))
        {
            RebuildGridItems();
        }
    }

    private void RebuildGridItems()
    {
        foreach (var section in Sections)
        {
            RebuildSection(section);
        }
    }

    private void RebuildSection(WhiteboardSection section)
    {
        section.Items.Clear();

        if (Settings.IsSortDescending)
        {
            section.Items.Add(BuildAddButtonWrapper(section.Size));
        }

        var visibleItems = WhiteboardItems
            .Where(i => i.LayoutSize == section.Size)
            .Where(i => Settings.ShowArchived || !i.IsArchived);

        if (Settings.IsSortDescending)
        {
            visibleItems = visibleItems.Reverse();
        }

        foreach (var item in visibleItems)
        {
            section.Items.Add(new GridItemWrapper
            {
                GridItemType = GridItemType.WhiteboardItem,
                LayoutSize = item.LayoutSize,
                Content = item
            });
        }

        if (!Settings.IsSortDescending)
        {
            section.Items.Add(BuildAddButtonWrapper(section.Size));
        }
    }

    private static GridItemWrapper BuildAddButtonWrapper(WhiteboardItemSize size) => new()
    {
        GridItemType = GridItemType.AddButton,
        LayoutSize = size,
        Content = null
    };

    public static IEnumerable<(TaskItemViewModel top, TaskItemViewModel? bottom)> PairUpTasks(IEnumerable<TaskItemViewModel> tasks)
    {
        using var e = tasks.GetEnumerator();
        while (e.MoveNext())
        {
            var top = e.Current;
            var bottom = e.MoveNext() ? e.Current : null;
            yield return (top, bottom);
        }
    }

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

    private async Task SaveDataAsync()
    {
        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        dispatcherQueue.TryEnqueue(() => IsSaving = true);

        try
        {
            await whiteboardItemRepository.SaveWhiteboardItemsAsync(WhiteboardItems);

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
                Debug.WriteLine($"Save failed: {ex.Message}");
            });
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var loadedSettings = await settingsService.LoadSettingsAsync();
            Settings = SettingsViewModel.FromModel(loadedSettings);

            var loadedItems = await whiteboardItemRepository.LoadWhiteboardItemsAsync();

            foreach (var item in loadedItems)
            {
                if (Settings.ShowArchived || !item.IsArchived)
                {
                    WhiteboardItems.Add(item);
                }
            }

            archivedItemsLoaded = Settings.ShowArchived;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Load failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnSettingsDataChanged(object? sender, EventArgs e)
    {
        _ = TriggerSettingsAutoSaveAsync();
    }

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

    public async Task ForceSaveAsync()
    {
        saveCts?.Cancel();
        settingsSaveCts?.Cancel();
        await Task.WhenAll(SaveDataAsync(), SaveSettingsAsync());
    }

    [RelayCommand]
    private async Task AddItemAsync(WhiteboardItemSize size)
    {
        workspace.ExitEditMode();

        var itemType = ItemTypeSizeRegistry.ItemTypesForSize(size).Cast<WhiteboardItemType?>().FirstOrDefault();
        if (itemType is null)
        {
            return;
        }

        WhiteboardItemViewModelBase newItem = itemType switch
        {
            WhiteboardItemType.Project => await CreateProjectAsync(),
            WhiteboardItemType.TaskItem => await CreateTaskAsync(),
            _ => throw new NotSupportedException($"Unsupported item type: {itemType}")
        };

        WhiteboardItems.Add(newItem);
        workspace.EnterEditMode(newItem);
    }

    private async Task<ProjectItemViewModel> CreateProjectAsync()
    {
        string imagePath = await imageStorageService.GetRandomDefaultImagePathAsync();

        var newProject = serviceProvider.GetRequiredService<ProjectItemViewModel>();
        newProject.Image = imagePath;
        newProject.ProjectSize = ProjectSize.Medium;
        newProject.Value = ProjectValue.Good;
        newProject.DueDate = null;

        await ApplyUniformToFillTransformAsync(newProject, imagePath);
        return newProject;
    }

    private async Task<TaskItemViewModel> CreateTaskAsync()
    {
        string imagePath = await imageStorageService.GetRandomDefaultImagePathAsync();

        var newTask = serviceProvider.GetRequiredService<TaskItemViewModel>();
        newTask.Image = imagePath;

        await ApplyUniformToFillTransformAsync(newTask, imagePath);
        return newTask;
    }

    [RelayCommand]
    private async Task ToggleShowArchivedAsync()
    {
        Settings.ShowArchived = !Settings.ShowArchived;

        if (Settings.ShowArchived && !archivedItemsLoaded)
        {
            await LoadArchivedItemsAsync();
        }

        RebuildGridItems();
    }

    private async Task LoadArchivedItemsAsync()
    {
        IsLoading = true;

        try
        {
            var loadedItems = await whiteboardItemRepository.LoadWhiteboardItemsAsync();
            var allItems = loadedItems.ToList();

            int currentCount = WhiteboardItems.Count;
            int loadedCount = allItems.Count;

            if (currentCount >= loadedCount)
            {
                return;
            }

            var archivedItems = allItems.Where(i => i.IsArchived);

            foreach (var item in archivedItems)
            {
                WhiteboardItems.Add(item);
            }

            archivedItemsLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load archived items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleSortOrder()
    {
        Settings.IsSortDescending = !Settings.IsSortDescending;
        RebuildGridItems();
    }

    [RelayCommand]
    private void RemoveWhiteboardItem(WhiteboardItemViewModelBase item)
    {
        if (SelectedItem == item)
        {
            ExitEditMode();
        }

        if (item.IsArchived)
        {
            WhiteboardItems.Remove(item);
        }
        else
        {
            item.IsArchived = true;
        }
    }

    [RelayCommand]
    private void ForceRemoveWhiteboardItem(WhiteboardItemViewModelBase item)
    {
        if (SelectedItem == item)
        {
            ExitEditMode();
        }

        WhiteboardItems.Remove(item);
    }

    [RelayCommand]
    private void ExitEditMode() => workspace.ExitEditMode();

    public async Task PasteImageFromClipboardAsync(XamlRoot xamlRoot)
    {
        try
        {
            string? title = null;
            string? subtitle = null;

            if (SelectedItem is ProjectItemViewModel project)
            {
                title = project.Title;
                subtitle = project.Subtitle;
            }
            else if (SelectedItem is TaskItemViewModel task)
            {
                title = task.Title;
                subtitle = task.Subtitle;
            }

            string defaultFileName = imageStorageService.GenerateFileName(title, subtitle);

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

            if (SelectedItem is ProjectItemViewModel projectItem)
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
            else if (SelectedItem is TaskItemViewModel taskItem)
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

    private async Task ApplyUniformToFillTransformAsync(ProjectItemViewModel project, string imageUri)
    {
        try
        {
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(imageUri);
            var (scale, offsetX, offsetY) = imageTransformService.CalculateDefaultTransform(
                width, height,
                ImageLayoutConstants.Project.ClipWidth,
                ImageLayoutConstants.Project.ClipHeight);

            project.ImageZoomFactor = scale;
            project.ImageOffsetX = offsetX;
            project.ImageOffsetY = offsetY;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply UniformToFill transform: {ex.Message}");
        }
    }

    private async Task ApplyUniformToFillTransformAsync(TaskItemViewModel item, string imageUri)
    {
        try
        {
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(imageUri);
            var (scale, offsetX, offsetY) = imageTransformService.CalculateDefaultTransform(
                width, height,
                ImageLayoutConstants.Task.ClipWidth,
                ImageLayoutConstants.Task.ClipHeight);

            item.ImageZoomFactor = scale;
            item.ImageOffsetX = offsetX;
            item.ImageOffsetY = offsetY;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply UniformToFill transform: {ex.Message}");
        }
    }
}
