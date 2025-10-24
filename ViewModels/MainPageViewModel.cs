using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly PrintService printService;
    private readonly WhiteboardItemRepository whiteboardItemRepository;
    private readonly SettingsService settingsService;
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;
    private CancellationTokenSource? saveCts;
    private CancellationTokenSource? settingsSaveCts;

    private const int AutoSaveDelayMs = 2000;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private DateTime? lastSaved;

    [ObservableProperty]
    private WhiteboardItemViewModelBase? selectedItem;

    [ObservableProperty]
    private int? activeSomedayMaybeItemIndex;

    [ObservableProperty]
    private SettingsViewModel settings = null!;

    public ObservableCollection<WhiteboardItemViewModelBase> WhiteboardItems { get; }
    public ObservableCollection<GridItemWrapper> GridItems { get; }

    public MainPageViewModel(PrintService printService, WhiteboardItemRepository whiteboardItemRepository, SettingsService settingsService, ImageStorageService imageStorageService, ImageTransformService imageTransformService, ImageDimensionService imageDimensionService)
    {
        this.printService = printService;
        this.whiteboardItemRepository = whiteboardItemRepository;
        this.settingsService = settingsService;
        this.imageStorageService = imageStorageService;
        this.imageTransformService = imageTransformService;
        this.imageDimensionService = imageDimensionService;

        Settings = new SettingsViewModel();

        WhiteboardItems = [];
        GridItems = [];

        GridItems.Add(new GridItemWrapper
        {
            GridItemType = GridItemType.AddButton,
            WhiteboardItemType = null,
            Content = null
        });

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
        var activeItems = WhiteboardItems.Where(i => !i.IsArchived);
        await printService.ShowPrintUIAsync(activeItems);
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
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (WhiteboardItemViewModelBase item in e.NewItems)
                    {
                        AddGridItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (WhiteboardItemViewModelBase item in e.OldItems)
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
        if (e.PropertyName == nameof(WhiteboardItemViewModelBase.IsArchived) && sender is WhiteboardItemViewModelBase item)
        {
            if (!Settings.ShowArchived)
            {
                if (item.IsArchived)
                {
                    RemoveGridItem(item);
                }
                else
                {
                    AddGridItem(item);
                }
            }
        }
    }

    private void AddGridItem(WhiteboardItemViewModelBase item)
    {
        if (!Settings.ShowArchived && item.IsArchived)
        {
            return;
        }

        if (GridItems.Any(w => w.WhiteboardItem == item))
        {
            return;
        }

        int insertIndex;

        if (Settings.IsSortDescending)
        {
            insertIndex = GridItems.Count > 0 && GridItems[0].GridItemType == GridItemType.AddButton ? 1 : 0;
        }
        else
        {
            insertIndex = GridItems.Count > 0 && GridItems[^1].GridItemType == GridItemType.AddButton
                ? GridItems.Count - 1
                : GridItems.Count;
        }

        GridItems.Insert(insertIndex, new GridItemWrapper
        {
            GridItemType = GridItemType.WhiteboardItem,
            WhiteboardItemType = item.GetItemType(),
            Content = item
        });
    }

    private void RemoveGridItem(WhiteboardItemViewModelBase item)
    {
        var wrapper = GridItems.FirstOrDefault(w => w.WhiteboardItem == item);
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
            GridItems.Add(new GridItemWrapper
            {
                GridItemType = GridItemType.AddButton,
                WhiteboardItemType = null,
                Content = null
            });
        }

        var itemsToDisplay = WhiteboardItems.Where(i => Settings.ShowArchived || !i.IsArchived);

        var sortedItems = Settings.IsSortDescending
            ? itemsToDisplay.Reverse()
            : itemsToDisplay;

        foreach (var item in sortedItems)
        {
            GridItems.Add(new GridItemWrapper
            {
                GridItemType = GridItemType.WhiteboardItem,
                WhiteboardItemType = item.GetItemType(),
                Content = item
            });
        }

        if (!Settings.IsSortDescending)
        {
            GridItems.Add(new GridItemWrapper
            {
                GridItemType = GridItemType.AddButton,
                WhiteboardItemType = null,
                Content = null
            });
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
            // Expected on debounce
        }
    }

    /// <summary>
    /// Saves whiteboard items to JSON file.
    /// </summary>
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

    /// <summary>
    /// Loads whiteboard items and settings from JSON files on startup.
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            var loadedSettings = await settingsService.LoadSettingsAsync();
            Settings = SettingsViewModel.FromModel(loadedSettings);

            var loadedItems = await whiteboardItemRepository.LoadWhiteboardItemsAsync();

            foreach (var item in loadedItems)
            {
                WhiteboardItems.Add(item);
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
            // Expected on debounce
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
    private void AddItem()
    {
        ExitEditMode();

        var selectorViewModel = new WhiteboardItemSelectorViewModel();

        selectorViewModel.ItemTypeSelected += OnSelectorItemTypeSelected;
        selectorViewModel.CancelRequested += OnSelectorCancelRequested;

        int insertIndex = Settings.IsSortDescending
            ? (GridItems.Count > 0 && GridItems[0].GridItemType == GridItemType.AddButton ? 1 : 0)
            : (GridItems.Count > 0 && GridItems[^1].GridItemType == GridItemType.AddButton ? GridItems.Count - 1 : GridItems.Count);

        GridItems.Insert(insertIndex, new GridItemWrapper
        {
            GridItemType = GridItemType.Selector,
            WhiteboardItemType = null,
            Content = selectorViewModel
        });
    }

    private void OnSelectorItemTypeSelected(object? sender, WhiteboardItemType itemType)
    {
        if (sender is WhiteboardItemSelectorViewModel selectorViewModel)
        {
            _ = CreateWhiteboardItemCommand.ExecuteAsync((selectorViewModel, itemType));
        }
    }

    private void OnSelectorCancelRequested(object? sender, EventArgs e)
    {
        if (sender is WhiteboardItemSelectorViewModel selectorViewModel)
        {
            CancelSelectorCommand.Execute(selectorViewModel);
        }
    }

    [RelayCommand]
    private async Task CreateWhiteboardItemAsync((WhiteboardItemSelectorViewModel Selector, WhiteboardItemType ItemType) parameters)
    {
        var wrapper = GridItems.FirstOrDefault(w => w.Selector == parameters.Selector);
        if (wrapper == null)
        {
            throw new InvalidOperationException($"GridItemWrapper not found for selector {parameters.Selector.Id}. The selector may have been removed before the item could be created.");
        }

        switch (parameters.ItemType)
        {
            case WhiteboardItemType.Project:
                await CreateProjectItemAsync(wrapper);
                break;
            case WhiteboardItemType.SomedayMaybe:
                await CreateSomedayMaybePairAsync(wrapper);
                break;
            case WhiteboardItemType.Goal:
                // Future implementation
                break;
            case WhiteboardItemType.Inspiration:
                // Future implementation
                break;
        }
    }

    private async Task CreateProjectItemAsync(GridItemWrapper wrapper)
    {
        string imagePath = await imageStorageService.GetRandomDefaultImagePathAsync();

        var newProject = new ProjectItemViewModel
        {
            Image = imagePath,
            Size = ProjectSize.Medium,
            Value = ProjectValue.Good,
            DueDate = null
        };

        await ApplyUniformToFillTransformAsync(newProject, imagePath);

        if (wrapper.Selector != null)
        {
            UnsubscribeFromSelector(wrapper.Selector);
        }

        GridItems.Remove(wrapper);

        WhiteboardItems.Add(newProject);

        EnterEditMode(newProject);
    }

    private async Task CreateSomedayMaybePairAsync(GridItemWrapper wrapper)
    {
        string topImagePath = await imageStorageService.GetRandomDefaultImagePathAsync();

        var topItem = new SomedayMaybeViewModel
        {
            Image = topImagePath,
            CreatedDate = DateTime.Today
        };

        await ApplyUniformToFillTransformAsync(topItem, topImagePath);

        var newPair = new SomedayMaybePairViewModel(imageStorageService, imageTransformService, imageDimensionService)
        {
            TopItem = topItem,
            BottomItem = null
        };

        if (wrapper.Selector != null)
        {
            UnsubscribeFromSelector(wrapper.Selector);
        }

        GridItems.Remove(wrapper);

        WhiteboardItems.Add(newPair);

        EnterEditMode(newPair);
    }

    [RelayCommand]
    private void CancelSelector(WhiteboardItemSelectorViewModel selector)
    {
        var wrapper = GridItems.FirstOrDefault(w => w.Selector == selector);
        if (wrapper != null)
        {
            UnsubscribeFromSelector(selector);
            GridItems.Remove(wrapper);
        }
    }

    private void UnsubscribeFromSelector(WhiteboardItemSelectorViewModel selector)
    {
        selector.ItemTypeSelected -= OnSelectorItemTypeSelected;
        selector.CancelRequested -= OnSelectorCancelRequested;
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
    private void RemoveWhiteboardItem(WhiteboardItemViewModelBase item)
    {
        if (SelectedItem == item)
        {
            ExitEditMode();
        }

        if (item.IsArchived)
        {
            // Permanently delete
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
    private void ReactivateItem(WhiteboardItemViewModelBase item)
    {
        item.IsArchived = false;
        ExitEditMode();
    }

    [RelayCommand]
    private void EnterEditMode(WhiteboardItemViewModelBase item)
    {
        if (SelectedItem != null && SelectedItem != item)
        {
            SelectedItem.IsEditing = false;
        }

        SelectedItem = item;
        item.IsEditing = true;
        ActiveSomedayMaybeItemIndex = null; // Reset when entering edit mode
    }

    [RelayCommand]
    private void ExitEditMode()
    {
        if (SelectedItem != null)
        {
            SelectedItem.IsEditing = false;
            SelectedItem = null;
        }
        ActiveSomedayMaybeItemIndex = null;
    }

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
            else if (SelectedItem is SomedayMaybePairViewModel pair)
            {
                var targetItem = ActiveSomedayMaybeItemIndex == 1 ? pair.BottomItem : pair.TopItem;
                title = targetItem?.Title;
                subtitle = targetItem?.Subtitle;
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
            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            if (SelectedItem is ProjectItemViewModel projectItem)
            {
                projectItem.ImageZoomFactor = scale;
                projectItem.ImageOffsetX = offsetX;
                projectItem.ImageOffsetY = offsetY;
                projectItem.Image = imageUri;
            }
            else if (SelectedItem is SomedayMaybePairViewModel pairItem)
            {
                var targetItem = ActiveSomedayMaybeItemIndex == 1 ? pairItem.BottomItem : pairItem.TopItem;
                if (targetItem != null)
                {
                    targetItem.ImageZoomFactor = scale;
                    targetItem.ImageOffsetX = offsetX;
                    targetItem.ImageOffsetY = offsetY;
                    targetItem.Image = imageUri;
                }
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
        if (SelectedItem == null)
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

            nint hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(file.Path);

            string? title = null;
            string? subtitle = null;

            if (SelectedItem is ProjectItemViewModel project)
            {
                title = project.Title;
                subtitle = project.Subtitle;
            }
            else if (SelectedItem is SomedayMaybePairViewModel pair)
            {
                var targetItem = ActiveSomedayMaybeItemIndex == 1 ? pair.BottomItem : pair.TopItem;
                title = targetItem?.Title;
                subtitle = targetItem?.Subtitle;
            }

            string fileName = imageStorageService.GenerateFileName(title, subtitle);
            string imageUri = await imageStorageService.SaveImageAsync(file.Path, fileName + Path.GetExtension(file.Path));

            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            if (SelectedItem is ProjectItemViewModel projectItem)
            {
                projectItem.ImageZoomFactor = scale;
                projectItem.ImageOffsetX = offsetX;
                projectItem.ImageOffsetY = offsetY;
                projectItem.Image = imageUri;
            }
            else if (SelectedItem is SomedayMaybePairViewModel pairItem)
            {
                var targetItem = ActiveSomedayMaybeItemIndex == 1 ? pairItem.BottomItem : pairItem.TopItem;
                if (targetItem != null)
                {
                    targetItem.ImageZoomFactor = scale;
                    targetItem.ImageOffsetX = offsetX;
                    targetItem.ImageOffsetY = offsetY;
                    targetItem.Image = imageUri;
                }
            }

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
    /// Applies UniformToFill transform to a someday maybe item based on its image URI.
    /// Used for default background images loaded from ms-appx:// URIs.
    /// </summary>
    private async Task ApplyUniformToFillTransformAsync(SomedayMaybeViewModel item, string imageUri)
    {
        try
        {
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(imageUri);
            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

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
