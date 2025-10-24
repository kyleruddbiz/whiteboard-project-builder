using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Navigation;
using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? window;

    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }

    /// <summary>
    /// Configures and builds the service provider for dependency injection.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Stateless services (no dependencies)
        services.AddSingleton<DataPersistenceService>();
        services.AddSingleton<ImageTransformService>();
        services.AddSingleton<ImageDimensionService>();

        // Services with dependencies
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ImageStorageService>();
        services.AddSingleton<WhiteboardItemRepository>();
        services.AddSingleton<PrintService>();

        // ViewModels - Singleton
        services.AddSingleton<MainPageViewModel>();

        // ViewModels - Transient (created on demand)
        services.AddTransient<WhiteboardItemSelectorViewModel>();
        services.AddTransient<ProjectItemViewModel>();
        services.AddTransient<SomedayMaybePairViewModel>();
        services.AddTransient<SomedayMaybeViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        window = new Window();
        MainWindow = window;

        if (window.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            window.Content = rootFrame;
        }

        _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);

        var presenter = window.AppWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
        {
            presenter.Maximize();
        }

        window.Activate();
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
