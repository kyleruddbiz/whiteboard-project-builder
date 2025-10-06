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
}