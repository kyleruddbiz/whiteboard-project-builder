using System.Diagnostics;
using Microsoft.UI.Xaml.Printing;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.ViewModels;
using WhiteboardProjectBuilder.Views;
using Windows.Foundation;
using Windows.Graphics.Printing;
using WinRT.Interop;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Service for printing whiteboard items using the Windows print system.
/// Accepts a list of <see cref="IPrintSlot"/> and packs them into pages using a Small-unit
/// grid (2 cols × 4 rows): Huge spans the whole page, Large spans a half-page row, Medium
/// fills a column-pair of cells, Small fills one cell.
/// </summary>
public class PrintService(PrintPagePackerService printPagePackerService)
{
    private PrintDocument? printDocument;
    private IPrintDocumentSource? printDocumentSource;
    private PrintManager? printManager;
    private readonly List<UIElement> printPages = new();
    private List<PrintPageLayout> pageLayouts = new();
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
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;

            printDocument.Paginate += PrintDocument_Paginate;
            printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;
            printDocument.AddPages += PrintDocument_AddPages;

            nint hWnd = WindowNative.GetWindowHandle(window);
            printManager = PrintManagerInterop.GetForWindow(hWnd);
            printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to register for printing: {ex.Message}");
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
        pageLayouts.Clear();

        // Safely clear canvas children - may already be disposed on app shutdown
        try
        {
            printCanvas?.Children.Clear();
        }
        catch (Exception)
        {
            // Canvas already disposed during shutdown - this is expected
        }
    }

    /// <summary>
    /// Displays the Windows print dialog for the specified print slots. Slots are packed
    /// onto pages largest-first via <see cref="PrintPagePackerService"/>.
    /// </summary>
    /// <param name="items">Collection of print slots to render</param>
    public async Task ShowPrintUIAsync(IEnumerable<IPrintSlot> items)
    {
        if (App.MainWindow == null)
        {
            Debug.WriteLine("MainWindow is null, cannot show print UI");
            return;
        }

        if (!PrintManager.IsSupported())
        {
            Debug.WriteLine("Printing is not supported on this device");
            return;
        }

        pageLayouts = printPagePackerService.BuildPrintPages(items).ToList();

        if (pageLayouts.Count == 0)
        {
            Debug.WriteLine("No items to print");
            return;
        }

        try
        {
            nint hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to show print UI: {ex.Message}");
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

        PrintPageDescription pageDescription = e.PrintTaskOptions.GetPageDescription(0);

        foreach (var layout in pageLayouts)
        {
            var printPageView = new PrintPageView
            {
                Layout = layout,
                Width = pageDescription.PageSize.Width,
                Height = pageDescription.PageSize.Height
            };

            printCanvas?.Children.Add(printPageView);

            // Force layout pass for proper rendering
            printPageView.Measure(new Size(pageDescription.PageSize.Width, pageDescription.PageSize.Height));
            printPageView.Arrange(new Rect(0, 0, pageDescription.PageSize.Width, pageDescription.PageSize.Height));
            printPageView.UpdateLayout();

            printPages.Add(printPageView);
        }

        if (printDocument != null)
        {
            printDocument.SetPreviewPageCount(printPages.Count, PreviewPageCountType.Final);
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
