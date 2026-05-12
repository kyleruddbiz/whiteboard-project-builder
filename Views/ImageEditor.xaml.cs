using Microsoft.UI.Xaml.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Services;
using Windows.Foundation;
using Windows.System;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ImageEditor : UserControl
{
    private const double ZoomSensitivityFactor = 300.0;
    private const double MaxZoomFactor = 3.0;
    private const double ArrowKeyStepSize = 1.0;

    private static readonly ImageTransformService transformService = new();

    private bool isDragging = false;
    private Point startPoint;
    private double startOffsetX;
    private double startOffsetY;
    private double startZoomFactor;
    private double imagePixelWidth;
    private double imagePixelHeight;

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

    public static readonly DependencyProperty ShowImageBorderProperty =
        DependencyProperty.Register(
            nameof(ShowImageBorder),
            typeof(bool),
            typeof(ImageEditor),
            new PropertyMetadata(true));

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

    public bool ShowImageBorder
    {
        get => (bool)GetValue(ShowImageBorderProperty);
        set => SetValue(ShowImageBorderProperty, value);
    }

    public string PanIcon => ImageEditMode.Pan.ToIcon();
    public string ZoomIcon => ImageEditMode.Zoom.ToIcon();

    public ImageEditor()
    {
        InitializeComponent();
        Loaded += ImageEditor_Loaded;
        InnerViewport.NaturalSizeAvailable += InnerViewport_NaturalSizeAvailable;
    }

    private void ImageEditor_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }

    private void InnerViewport_NaturalSizeAvailable(ImageViewport sender, Size naturalSize)
    {
        imagePixelWidth = naturalSize.Width;
        imagePixelHeight = naturalSize.Height;
        // A new image with prior offsets/zoom may now be out of bounds — re-clamp.
        ApplyTransform(ZoomFactor, OffsetX, OffsetY);
    }

    private void ApplyTransform(double zoom, double offsetX, double offsetY)
    {
        zoom = Math.Clamp(zoom, ImageTransformService.MinZoomFactor, MaxZoomFactor);

        Size viewport = InnerViewport.ViewportSize;
        if (imagePixelWidth > 0 && imagePixelHeight > 0 && viewport.Width > 0 && viewport.Height > 0)
        {
            (offsetX, offsetY) = transformService.ClampOffset(
                offsetX, offsetY, zoom,
                imagePixelWidth, imagePixelHeight,
                viewport.Width, viewport.Height);
        }

        ZoomFactor = zoom;
        OffsetX = offsetX;
        OffsetY = offsetY;
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

    private void ArrowKeyAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (EditMode != ImageEditMode.Pan)
        {
            return;
        }

        double newOffsetX = OffsetX;
        double newOffsetY = OffsetY;
        switch (sender.Key)
        {
            case VirtualKey.Left:
                newOffsetX -= ArrowKeyStepSize;
                break;
            case VirtualKey.Right:
                newOffsetX += ArrowKeyStepSize;
                break;
            case VirtualKey.Up:
                newOffsetY -= ArrowKeyStepSize;
                break;
            case VirtualKey.Down:
                newOffsetY += ArrowKeyStepSize;
                break;
        }

        ApplyTransform(ZoomFactor, newOffsetX, newOffsetY);
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
        if (!isDragging)
            return;

        var currentPoint = e.GetCurrentPoint(Viewport).Position;
        double deltaX = currentPoint.X - startPoint.X;
        double deltaY = currentPoint.Y - startPoint.Y;

        if (EditMode == ImageEditMode.Pan)
        {
            ApplyTransform(ZoomFactor, startOffsetX + deltaX, startOffsetY + deltaY);

            // Update start values for next movement segment
            startPoint = currentPoint;
            startOffsetX = OffsetX;
            startOffsetY = OffsetY;
        }
        else if (EditMode == ImageEditMode.Zoom)
        {
            // Zoom with dominant axis: use the larger of horizontal or vertical movement
            double absDeltaX = Math.Abs(deltaX);
            double absDeltaY = Math.Abs(deltaY);
            double dominantDelta = absDeltaX > absDeltaY ? -deltaX : deltaY;

            // Zoom: right/up = zoom in, left/down = zoom out
            double scaleDelta = 1 - (dominantDelta / ZoomSensitivityFactor);
            double newZoomFactor = Math.Clamp(startZoomFactor * scaleDelta, ImageTransformService.MinZoomFactor, MaxZoomFactor);

            // Scale offsets proportionally to keep the image's visual centre stable through the zoom,
            // then clamp — the new (smaller or larger) zoom may push the scaled offsets out of bounds.
            double scaleRatio = newZoomFactor / startZoomFactor;
            ApplyTransform(newZoomFactor, startOffsetX * scaleRatio, startOffsetY * scaleRatio);

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

    private void Viewport_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    public void SetFocus()
    {
        PanButton.Focus(FocusState.Programmatic);
    }
}
