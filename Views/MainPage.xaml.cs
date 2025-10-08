using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public partial class MainPage : Page
{
    private readonly PrintService printService;
    private readonly DataPersistenceService dataPersistenceService;

    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        printService = new PrintService();
        dataPersistenceService = new DataPersistenceService();
        ViewModel = new MainPageViewModel(printService, dataPersistenceService);
        this.InitializeComponent();

        if (App.MainWindow != null)
        {
            printService.RegisterForPrinting(App.MainWindow, PrintCanvas);
        }

        this.Unloaded += MainPage_Unloaded;
        this.PointerPressed += MainPage_PointerPressed;
    }

    private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.SelectedProject?.IsEditing != true)
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
        this.Unloaded -= MainPage_Unloaded;
    }

    private void GridView_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        // Exit edit mode on currently editing item before changing selection
        if (ViewModel.SelectedProject?.IsEditing == true)
        {
            ViewModel.SelectedProject.IsEditing = false;
        }

        var gridView = (GridView)sender;
        if (gridView.SelectedItem is GridItemWrapper wrapper)
        {
            // Only set SelectedProject if it's an actual project (not the Add button)
            ViewModel.SelectedProject = wrapper.IsAddButton ? null : wrapper.ProjectItem;
        }
        else
        {
            ViewModel.SelectedProject = null;
        }
    }

    private async void PasteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        // Check if clipboard contains an image
        var dataPackageView = Clipboard.GetContent();
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
        {
            // No image in clipboard, allow default paste behavior
            args.Handled = false;
            return;
        }

        // Image found, handle paste
        args.Handled = true;
        await ViewModel.PasteImageFromClipboardAsync(this.XamlRoot);
    }

    private void ProjectItemView_EditRequested(object? sender, ProjectItemViewModel e)
    {
        ViewModel.EnterEditModeCommand.Execute(e);
    }
}