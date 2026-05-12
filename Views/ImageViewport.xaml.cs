using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ImageViewport : UserControl
{
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(string),
            typeof(ImageViewport),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(
            nameof(OffsetX),
            typeof(double),
            typeof(ImageViewport),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(ImageViewport),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty ZoomFactorProperty =
        DependencyProperty.Register(
            nameof(ZoomFactor),
            typeof(double),
            typeof(ImageViewport),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty ShowImageBorderProperty =
        DependencyProperty.Register(
            nameof(ShowImageBorder),
            typeof(bool),
            typeof(ImageViewport),
            new PropertyMetadata(true));

    /// <summary>
    /// Raised when the underlying bitmap finishes loading and its natural pixel dimensions
    /// are known. Subscribers (e.g. <see cref="ImageEditor"/>) use this to compute clamping
    /// bounds for pan and zoom.
    /// </summary>
    public event TypedEventHandler<ImageViewport, Size>? NaturalSizeAvailable;

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

    public bool ShowImageBorder
    {
        get => (bool)GetValue(ShowImageBorderProperty);
        set => SetValue(ShowImageBorderProperty, value);
    }

    public Thickness GetBorderThickness(bool showImageBorder) => showImageBorder ? new Thickness(2) : new Thickness(0);

    /// <summary>
    /// The actual on-screen pixel size of the clipped image area (excludes the 2px black border).
    /// Returns <see cref="Size.Empty"/> if the control hasn't laid out yet.
    /// </summary>
    public Size ViewportSize =>
        ClipContainer is { ActualWidth: > 0, ActualHeight: > 0 }
            ? new Size(ClipContainer.ActualWidth, ClipContainer.ActualHeight)
            : Size.Empty;

    public ImageViewport()
    {
        InitializeComponent();
        Loaded += ImageViewport_Loaded;
        SizeChanged += ImageViewport_SizeChanged;
        ImageElement.ImageOpened += ImageElement_ImageOpened;
    }

    private void ImageViewport_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateClip();
    }

    private void ImageViewport_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateClip();
    }

    private void ImageElement_ImageOpened(object sender, RoutedEventArgs e)
    {
        if (ImageElement.Source is BitmapImage bitmap && bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0)
        {
            NaturalSizeAvailable?.Invoke(this, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        }
    }

    private void UpdateClip()
    {
        if (ClipContainer != null && ClipContainer.ActualWidth > 0 && ClipContainer.ActualHeight > 0)
        {
            ClipContainer.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, ClipContainer.ActualWidth, ClipContainer.ActualHeight)
            };
        }
    }
}
