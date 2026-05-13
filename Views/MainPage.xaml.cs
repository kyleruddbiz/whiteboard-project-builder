using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
        ViewModel = App.Services.GetRequiredService<MainPageViewModel>();
        printService = App.Services.GetRequiredService<PrintService>();

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

        // PreviewKeyDown (tunneling) catches Ctrl+Tab before focused TextBoxes / the focus
        // traversal layer consume it; KeyboardAccelerators don't fire reliably for Tab from
        // inside a TextBox.
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(Page_PreviewKeyDown_Handler), true);
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

    private void Page_PreviewKeyDown_Handler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Tab)
        {
            return;
        }

        bool isCtrlDown = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Control)
            .HasFlag(CoreVirtualKeyStates.Down);

        if (!isCtrlDown)
        {
            return;
        }

        bool isShiftDown = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Shift)
            .HasFlag(CoreVirtualKeyStates.Down);

        if (isShiftDown)
        {
            ViewModel.MoveToPreviousItemCommand.Execute(null);
        }
        else
        {
            ViewModel.MoveToNextItemCommand.Execute(null);
        }

        e.Handled = true;
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
}
