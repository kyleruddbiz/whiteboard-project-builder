using Microsoft.UI.Xaml.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ImageEditor : UserControl
{
    private const double ZoomSensitivityFactor = 300.0;
    private const double MinZoomFactor = 0.5;
    private const double MaxZoomFactor = 3.0;

    private bool isDragging = false;
    private Windows.Foundation.Point startPoint;
    private double startOffsetX;
    private double startOffsetY;
    private double startZoomFactor;

    public event EventHandler? ImageReplaceRequested;

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(string),
            typeof(ImageEditor),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(
            nameof(OffsetX),
            typeof(double),
            typeof(ImageEditor),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(ImageEditor),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty ZoomFactorProperty =
        DependencyProperty.Register(
            nameof(ZoomFactor),
            typeof(double),
            typeof(ImageEditor),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty EditModeProperty =
        DependencyProperty.Register(
            nameof(EditMode),
            typeof(ImageEditMode),
            typeof(ImageEditor),
            new PropertyMetadata(ImageEditMode.Pan, OnEditModeChanged));

    public string ImageSource
    {
        get => (string)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    public double ZoomFactor
    {
        get => (double)GetValue(ZoomFactorProperty);
        set => SetValue(ZoomFactorProperty, value);
    }

    public ImageEditMode EditMode
    {
        get => (ImageEditMode)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public string PanIcon => ImageEditMode.Pan.ToIcon();
    public string ZoomIcon => ImageEditMode.Zoom.ToIcon();

    public ImageEditor()
    {
        this.InitializeComponent();
        this.Loaded += ImageEditor_Loaded;
    }

    private void ImageEditor_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageEditor editor)
        {
            editor.UpdateButtonStyles();
        }
    }

    private void UpdateButtonStyles()
    {
        if (PanButton != null && ZoomButton != null)
        {
            PanButton.Style = EditMode == ImageEditMode.Pan
                ? (Style)Application.Current.Resources["AccentButtonStyle"]
                : (Style)Application.Current.Resources["DefaultButtonStyle"];
            ZoomButton.Style = EditMode == ImageEditMode.Zoom
                ? (Style)Application.Current.Resources["AccentButtonStyle"]
                : (Style)Application.Current.Resources["DefaultButtonStyle"];
        }
    }

    private void PanButton_Click(object sender, RoutedEventArgs e)
    {
        EditMode = ImageEditMode.Pan;
    }

    private void ZoomButton_Click(object sender, RoutedEventArgs e)
    {
        EditMode = ImageEditMode.Zoom;
    }

    private void PanAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        EditMode = ImageEditMode.Pan;
        args.Handled = true;
    }

    private void ZoomAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        EditMode = ImageEditMode.Zoom;
        args.Handled = true;
    }

    private void Viewport_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        isDragging = true;
        startPoint = e.GetCurrentPoint(Viewport).Position;
        startOffsetX = OffsetX;
        startOffsetY = OffsetY;
        startZoomFactor = ZoomFactor;
        Viewport.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Viewport_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!isDragging) return;

        var currentPoint = e.GetCurrentPoint(Viewport).Position;
        var deltaX = currentPoint.X - startPoint.X;
        var deltaY = currentPoint.Y - startPoint.Y;

        if (EditMode == ImageEditMode.Pan)
        {
            OffsetX = startOffsetX + deltaX;
            OffsetY = startOffsetY + deltaY;

            // Update start values for next movement segment
            startPoint = currentPoint;
            startOffsetX = OffsetX;
            startOffsetY = OffsetY;
        }
        else if (EditMode == ImageEditMode.Zoom)
        {
            // Zoom with dominant axis: use the larger of horizontal or vertical movement
            var absDeltaX = Math.Abs(deltaX);
            var absDeltaY = Math.Abs(deltaY);
            var dominantDelta = absDeltaX > absDeltaY ? -deltaX : deltaY;

            // Zoom: right/up = zoom in, left/down = zoom out
            var scaleDelta = 1 - (dominantDelta / ZoomSensitivityFactor);
            var newZoomFactor = startZoomFactor * scaleDelta;
            newZoomFactor = Math.Clamp(newZoomFactor, MinZoomFactor, MaxZoomFactor);

            // Adjust offsets proportionally to maintain visual center
            var scaleRatio = newZoomFactor / startZoomFactor;
            var newOffsetX = startOffsetX * scaleRatio;
            var newOffsetY = startOffsetY * scaleRatio;

            ZoomFactor = newZoomFactor;
            OffsetX = newOffsetX;
            OffsetY = newOffsetY;

            // Update start values for next movement segment
            startPoint = currentPoint;
            startZoomFactor = ZoomFactor;
            startOffsetX = OffsetX;
            startOffsetY = OffsetY;
        }

        e.Handled = true;
    }

    private void Viewport_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        isDragging = false;
        Viewport.ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private async void Viewport_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        await ClipboardService.CopyImagesFolderPathToClipboardAsync();
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
}
