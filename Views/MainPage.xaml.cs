using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace WhiteboardProjectBuilder.Views;

public partial class MainPage : Page
{
    private readonly PrintService printService;
    private readonly DataPersistenceService dataPersistenceService;
    private readonly SettingsService settingsService;

    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        printService = new PrintService();
        dataPersistenceService = new DataPersistenceService();
        settingsService = new SettingsService(dataPersistenceService);
        ViewModel = new MainPageViewModel(printService, dataPersistenceService, settingsService);
        InitializeComponent();

        if (App.MainWindow != null)
        {
            printService.RegisterForPrinting(App.MainWindow, PrintCanvas);
        }

        Unloaded += MainPage_Unloaded;
        PointerPressed += MainPage_PointerPressed;
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
        Unloaded -= MainPage_Unloaded;
    }

    private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedProject?.IsEditing == true)
        {
            ViewModel.SelectedProject.IsEditing = false;
        }

        var gridView = (GridView)sender;
        if (gridView.SelectedItem is GridItemWrapper wrapper && wrapper.ProjectItem != null)
        {
            ViewModel.SelectedProject = wrapper.ProjectItem;
        }
        else
        {
            ViewModel.SelectedProject = null;
        }
    }

    private async void PasteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var dataPackageView = Clipboard.GetContent();
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
        {
            args.Handled = false;
            return;
        }

        args.Handled = true;
        await ViewModel.PasteImageFromClipboardAsync(XamlRoot);
    }

    private void ProjectItemView_EditRequested(object? sender, ProjectItemViewModel e)
    {
        ViewModel.EnterEditModeCommand.Execute(e);
    }

    private async void ProjectItemView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        await ViewModel.ReplaceImageAsync(XamlRoot);
    }

    private void ProjectItemView_ReactivateRequested(object? sender, ProjectItemViewModel e)
    {
        ViewModel.ReactivateProjectCommand.Execute(e);
    }
}
