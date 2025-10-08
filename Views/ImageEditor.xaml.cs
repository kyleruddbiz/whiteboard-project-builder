using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ImageEditor : UserControl
{
    private bool isDragging = false;
    private Windows.Foundation.Point startPoint;
    private double startOffsetX;
    private double startOffsetY;
    private double startScale;

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
            new PropertyMetadata(0.0, OnTransformChanged));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(ImageEditor),
            new PropertyMetadata(0.0, OnTransformChanged));

    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register(
            nameof(Scale),
            typeof(double),
            typeof(ImageEditor),
            new PropertyMetadata(1.0, OnTransformChanged));

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

    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
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
        this.SizeChanged += ImageEditor_SizeChanged;
    }

    private void ImageEditor_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateTransform();
        UpdateButtonStyles();
        UpdateClip();
    }

    private void ImageEditor_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateClip();
    }

    private void UpdateClip()
    {
        if (ClipContainer != null && ClipContainer.ActualWidth > 0 && ClipContainer.ActualHeight > 0)
        {
            ClipContainer.Clip = new RectangleGeometry
            {
                Rect = new Windows.Foundation.Rect(0, 0, ClipContainer.ActualWidth, ClipContainer.ActualHeight)
            };
        }
    }

    private static void OnTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageEditor editor)
        {
            editor.UpdateTransform();
        }
    }

    private static void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageEditor editor)
        {
            editor.UpdateButtonStyles();
        }
    }

    private void UpdateTransform()
    {
        if (ZoomTransform != null && PanTransform != null)
        {
            ZoomTransform.ScaleX = Scale;
            ZoomTransform.ScaleY = Scale;
            PanTransform.X = OffsetX;
            PanTransform.Y = OffsetY;
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
        startScale = Scale;
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
            // Zoom with vertical drag: down = zoom out, up = zoom in
            var scaleDelta = 1 - (deltaY / 300.0);
            var newScale = startScale * scaleDelta;
            newScale = Math.Clamp(newScale, 0.5, 3.0);

            // Adjust offsets proportionally to maintain visual center
            var scaleRatio = newScale / startScale;
            var newOffsetX = startOffsetX * scaleRatio;
            var newOffsetY = startOffsetY * scaleRatio;

            Scale = newScale;
            OffsetX = newOffsetX;
            OffsetY = newOffsetY;

            // Update start values for next movement segment
            startPoint = currentPoint;
            startScale = Scale;
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
}
