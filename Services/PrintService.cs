using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Printing;
using WhiteboardProjectBuilder.ViewModels;
using WhiteboardProjectBuilder.Views;
using Windows.Graphics.Printing;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for printing whiteboard project items using the Windows print system.
/// Supports printing multiple ProjectItemViewModel instances with up to 4 items per page in a 2x2 grid layout.
/// </summary>
public class PrintService
{
    private PrintDocument? printDocument;
    private IPrintDocumentSource? printDocumentSource;
    private PrintManager? printManager;
    private List<UIElement> printPages = new();
    private List<ProjectItemViewModel> itemsToPrint = new();
    private Canvas? printCanvas;

    /// <summary>
    /// Registers the print service with the Windows PrintManager for the application's main window.
    /// Must be called before ShowPrintUIAsync.
    /// </summary>
    /// <param name="window">The application window to associate with printing</param>
    /// <param name="canvas">The invisible canvas element used for rendering print pages</param>
    public void RegisterForPrinting(Window window, Canvas canvas)
    {
        printCanvas = canvas;

        try
        {
            // Create PrintDocument early
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;

            printDocument.Paginate += PrintDocument_Paginate;
            printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;
            printDocument.AddPages += PrintDocument_AddPages;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            printManager = PrintManagerInterop.GetForWindow(hWnd);
            printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to register for printing: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregisters print event handlers and cleans up resources.
    /// Should be called when the page is unloaded.
    /// </summary>
    public void UnregisterForPrinting()
    {
        if (printDocument != null)
        {
            printDocument.Paginate -= PrintDocument_Paginate;
            printDocument.GetPreviewPage -= PrintDocument_GetPreviewPage;
            printDocument.AddPages -= PrintDocument_AddPages;
        }

        if (printManager != null)
        {
            printManager.PrintTaskRequested -= PrintManager_PrintTaskRequested;
        }

        printPages.Clear();
        itemsToPrint.Clear();
        printCanvas?.Children.Clear();
    }

    /// <summary>
    /// Displays the Windows print dialog for the specified project items.
    /// </summary>
    /// <param name="items">Collection of ProjectItemViewModel instances to print</param>
    public async Task ShowPrintUIAsync(IEnumerable<ProjectItemViewModel> items)
    {
        if (App.MainWindow == null)
        {
            System.Diagnostics.Debug.WriteLine("MainWindow is null, cannot show print UI");
            return;
        }

        if (!PrintManager.IsSupported())
        {
            System.Diagnostics.Debug.WriteLine("Printing is not supported on this device");
            return;
        }

        itemsToPrint = items.ToList();

        if (itemsToPrint.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No items to print");
            return;
        }

        try
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show print UI: {ex.Message}");
        }
    }

    private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
    {
        PrintTask printTask = args.Request.CreatePrintTask("Whiteboard Project Items", sourceRequestedArgs =>
        {
            if (printDocumentSource != null)
            {
                sourceRequestedArgs.SetSource(printDocumentSource);
            }
        });

        printTask.Completed += PrintTask_Completed;
    }

    private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
    {
        printPages.Clear();
        printCanvas?.Children.Clear();

        int itemsPerPage = 4;
        int totalPages = (int)Math.Ceiling(itemsToPrint.Count / (double)itemsPerPage);

        PrintPageDescription pageDescription = e.PrintTaskOptions.GetPageDescription(0);

        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            int startIndex = pageIndex * itemsPerPage;
            int itemsOnThisPage = Math.Min(itemsPerPage, itemsToPrint.Count - startIndex);

            List<ProjectItemViewModel> pageItems = itemsToPrint
                .Skip(startIndex)
                .Take(itemsOnThisPage)
                .ToList();

            var printPageView = new PrintPageView
            {
                Items = pageItems,
                Width = pageDescription.PageSize.Width,
                Height = pageDescription.PageSize.Height
            };

            printCanvas?.Children.Add(printPageView);

            // Force layout pass for proper rendering
            printPageView.Measure(new Windows.Foundation.Size(pageDescription.PageSize.Width, pageDescription.PageSize.Height));
            printPageView.Arrange(new Windows.Foundation.Rect(0, 0, pageDescription.PageSize.Width, pageDescription.PageSize.Height));
            printPageView.UpdateLayout();

            printPages.Add(printPageView);
        }

        if (printDocument != null)
        {
            printDocument.SetPreviewPageCount(totalPages, PreviewPageCountType.Final);
        }
    }

    private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
    {
        if (printDocument != null && e.PageNumber > 0 && e.PageNumber <= printPages.Count)
        {
            printDocument.SetPreviewPage(e.PageNumber, printPages[e.PageNumber - 1]);
        }
    }

    private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
    {
        if (printDocument != null)
        {
            foreach (UIElement page in printPages)
            {
                printDocument.AddPage(page);
            }

            printDocument.AddPagesComplete();
        }
    }

    private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
    {
        printPages.Clear();

        if (printCanvas != null)
        {
            printCanvas.DispatcherQueue.TryEnqueue(() =>
            {
                printCanvas.Children.Clear();
            });
        }
    }
}
