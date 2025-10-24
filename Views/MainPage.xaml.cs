using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace WhiteboardProjectBuilder.Views;

public partial class MainPage : Page
{
    private readonly PrintService printService;

    public static readonly DependencyProperty IsShiftPressedProperty =
        DependencyProperty.Register(
            nameof(IsShiftPressed),
            typeof(bool),
            typeof(MainPage),
            new PropertyMetadata(false));

    public MainPageViewModel ViewModel { get; }

    public bool IsShiftPressed
    {
        get => (bool)GetValue(IsShiftPressedProperty);
        set => SetValue(IsShiftPressedProperty, value);
    }

    public MainPage()
    {
        printService = new PrintService();

        var imageTransformService = new ImageTransformService();
        var imageDimensionService = new ImageDimensionService();
        var dataPersistenceService = new DataPersistenceService();

        var settingsService = new SettingsService(dataPersistenceService);
        var imageStorageService = new ImageStorageService(settingsService);

        var whiteboardItemRepository = new WhiteboardItemRepository(
            dataPersistenceService,
            imageStorageService,
            imageTransformService,
            imageDimensionService);

        ViewModel = new MainPageViewModel(
            printService,
            whiteboardItemRepository,
            settingsService,
            imageStorageService,
            imageTransformService,
            imageDimensionService);

        InitializeComponent();

        if (App.MainWindow != null)
        {
            printService.RegisterForPrinting(App.MainWindow, PrintCanvas);
            App.MainWindow.Activated += MainWindow_Activated;
        }

        Unloaded += MainPage_Unloaded;
        PointerPressed += MainPage_PointerPressed;
    }

    private void MainWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.Deactivated)
        {
            IsShiftPressed = false;
        }
        else if (e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.CodeActivated ||
                 e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.PointerActivated)
        {
            bool isShiftDown = InputKeyboardSource
                .GetKeyStateForCurrentThread(VirtualKey.Shift)
                .HasFlag(CoreVirtualKeyStates.Down);

            IsShiftPressed = isShiftDown;
        }
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Shift)
        {
            IsShiftPressed = true;
        }
    }

    private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Shift)
        {
            IsShiftPressed = false;
        }
    }

    private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.SelectedItem?.IsEditing != true)
            return;

        var point = e.GetCurrentPoint(this);
        var elements = VisualTreeHelper.FindElementsInHostCoordinates(point.Position, this);

        bool clickedInsideEditView = elements.Any(el =>
            el is FrameworkElement fe &&
            (fe.Name == "EditView" || IsDescendantOfName(el, "EditView")));

        if (!clickedInsideEditView)
        {
            ViewModel.ExitEditModeCommand.Execute(null);
        }
    }

    private bool IsDescendantOfName(DependencyObject element, string ancestorName)
    {
        while (element != null)
        {
            if (element is FrameworkElement fe && fe.Name == ancestorName)
                return true;
            element = VisualTreeHelper.GetParent(element);
        }
        return false;
    }

    private async void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.ForceSaveAsync();

        printService.UnregisterForPrinting();

        if (App.MainWindow != null)
        {
            App.MainWindow.Activated -= MainWindow_Activated;
        }

        Unloaded -= MainPage_Unloaded;
    }

    private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedItem?.IsEditing == true)
        {
            ViewModel.SelectedItem.IsEditing = false;
        }

        var gridView = (GridView)sender;
        if (gridView.SelectedItem is GridItemWrapper wrapper && wrapper.WhiteboardItem != null)
        {
            ViewModel.SelectedItem = wrapper.WhiteboardItem;
        }
        else
        {
            ViewModel.SelectedItem = null;
        }
    }

    private void ProjectItemView_EditRequested(object? sender, ProjectItemViewModel e)
    {
        ViewModel.EnterEditModeCommand.Execute(e);
    }

    private async void ProjectItemView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        ViewModel.ActiveSomedayMaybeItemIndex = null;
        await ViewModel.ReplaceImageAsync(XamlRoot);
    }

    private void ProjectItemView_ReactivateRequested(object? sender, ProjectItemViewModel e)
    {
        ViewModel.ReactivateItemCommand.Execute(e);
    }

    private void SomedayMaybePairView_EditRequested(object? sender, EventArgs e)
    {
        if (sender is SomedayMaybePairView view && view.ViewModel != null)
        {
            ViewModel.EnterEditModeCommand.Execute(view.ViewModel);
        }
    }

    private async void SomedayMaybePairView_ImageReplaceRequested(object? sender, int itemIndex)
    {
        ViewModel.ActiveSomedayMaybeItemIndex = itemIndex;
        await ViewModel.ReplaceImageAsync(XamlRoot);
    }

    private void SomedayMaybePairView_ReactivateRequested(object? sender, EventArgs e)
    {
        if (sender is SomedayMaybePairView view && view.ViewModel != null)
        {
            ViewModel.ReactivateItemCommand.Execute(view.ViewModel);
        }
    }
}
