using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
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

    public IReadOnlyList<WhiteboardSectionViewModel> Sections { get; }

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
        WhiteboardItemWorkspaceViewModel workspace,
        IServiceProvider serviceProvider)
    {
        this.printService = printService;
        this.whiteboardItemRepository = whiteboardItemRepository;
        this.settingsService = settingsService;
        this.imageStorageService = imageStorageService;
        this.workspace = workspace;
        this.serviceProvider = serviceProvider;

        Settings = serviceProvider.GetRequiredService<SettingsViewModel>();

        WhiteboardItems = [];
        Sections = Enum.GetValues<WhiteboardItemSize>()
            .OrderByDescending(size => size)
            .Select(size => new WhiteboardSectionViewModel(size, workspace))
            .ToList();

        RebuildGridItems();

        WhiteboardItems.CollectionChanged += WhiteboardItems_CollectionChanged;
        WhiteboardItems.CollectionChanged += OnWhiteboardItemsCollectionChanged;

        workspace.ItemCreated += OnWorkspaceItemCreated;
        workspace.SelectorAdded += OnWorkspaceSelectorAdded;
        workspace.SelectorRemoved += OnWorkspaceSelectorRemoved;

        _ = LoadDataAsync();
    }

    private void OnWorkspaceItemCreated(object? sender, WhiteboardItemViewModelBase item)
    {
        WhiteboardItems.Add(item);
    }

    private void OnWorkspaceSelectorAdded(object? sender, (WhiteboardItemSize Size, WhiteboardItemSelectorViewModel Selector) e)
    {
        var section = Sections.First(s => s.Size == e.Size);
        section.Selectors.Add(e.Selector);
        RebuildSection(section);
    }

    private void OnWorkspaceSelectorRemoved(object? sender, (WhiteboardItemSize Size, WhiteboardItemSelectorViewModel Selector) e)
    {
        var section = Sections.First(s => s.Size == e.Size);
        section.Selectors.Remove(e.Selector);
        RebuildSection(section);
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

    public static IEnumerable<IPrintSlot> BuildPrintSlots(IEnumerable<WhiteboardItemViewModelBase> items) =>
        items.OfType<IPrintSlot>().OrderByDescending(slot => slot.LayoutSize);

    [RelayCommand]
    private async Task OpenImagesFolderAsync()
    {
        try
        {
            string folderPath = await imageStorageService.GetImagesFolderPathAsync();
            Process.Start("explorer.exe", folderPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open images folder: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanPasteImage))]
    private Task PasteImageAsync(XamlRoot xamlRoot) => workspace.PasteImageAsync(xamlRoot);

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

    private void RebuildSection(WhiteboardSectionViewModel section)
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

        foreach (var selector in section.Selectors)
        {
            section.Items.Add(new GridItemWrapper
            {
                GridItemType = GridItemType.Selector,
                LayoutSize = section.Size,
                Content = selector
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
    private Task AddItemAsync(WhiteboardItemSize size) => workspace.RequestAddItemAsync(size);

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
}
