using Microsoft.UI.Xaml.Input;
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
    }

    private async void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.ForceSaveAsync();

        printService.UnregisterForPrinting();
        this.Unloaded -= MainPage_Unloaded;
    }

    private void GridView_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
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
}