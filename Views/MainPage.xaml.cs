using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public partial class MainPage : Page
{
    private readonly PrintService printService;

    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        printService = new PrintService();
        ViewModel = new MainPageViewModel(printService);
        this.InitializeComponent();

        if (App.MainWindow != null)
        {
            printService.RegisterForPrinting(App.MainWindow, PrintCanvas);
        }

        this.Unloaded += MainPage_Unloaded;
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        printService.UnregisterForPrinting();
        this.Unloaded -= MainPage_Unloaded;
    }
}
