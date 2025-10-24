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

    public static readonly DependencyProperty IsCtrlPressedProperty =
        DependencyProperty.Register(
            nameof(IsCtrlPressed),
            typeof(bool),
            typeof(MainPage),
            new PropertyMetadata(false));

    public MainPageViewModel ViewModel { get; }

    public bool IsCtrlPressed
    {
        get => (bool)GetValue(IsCtrlPressedProperty);
        set => SetValue(IsCtrlPressedProperty, value);
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

        Loaded += MainPage_Loaded;
        Unloaded += MainPage_Unloaded;
        PointerPressed += MainPage_PointerPressed;

        // Use AddHandler to listen to keyboard events even when handled by child elements
        AddHandler(KeyDownEvent, new KeyEventHandler(Page_KeyDown_Handler), true);
        AddHandler(KeyUpEvent, new KeyEventHandler(Page_KeyUp_Handler), true);
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Focus the root grid so it receives keyboard events
        RootGrid.Focus(FocusState.Programmatic);
    }

    private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        // When user taps anywhere on the grid (empty areas), focus it to enable keyboard events
        RootGrid.Focus(FocusState.Programmatic);
    }

    private void MainWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.Deactivated)
        {
            IsCtrlPressed = false;
        }
        else if (e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.CodeActivated ||
                 e.WindowActivationState == Microsoft.UI.Xaml.WindowActivationState.PointerActivated)
        {
            bool isCtrlDown = InputKeyboardSource
                .GetKeyStateForCurrentThread(VirtualKey.Control)
                .HasFlag(CoreVirtualKeyStates.Down);

            IsCtrlPressed = isCtrlDown;
        }
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Control)
        {
            IsCtrlPressed = true;
        }
    }

    private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Control)
        {
            IsCtrlPressed = false;
        }
    }

    private void Page_KeyDown_Handler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Control)
        {
            IsCtrlPressed = true;
        }
    }

    private void Page_KeyUp_Handler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Control)
        {
            IsCtrlPressed = false;
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

            // Focus the root grid to keep keyboard events working
            RootGrid.Focus(FocusState.Programmatic);
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
