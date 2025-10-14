using Microsoft.UI.Xaml.Media;
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

    public ImageViewport()
    {
        InitializeComponent();
        Loaded += ImageViewport_Loaded;
        SizeChanged += ImageViewport_SizeChanged;
    }

    private void ImageViewport_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateClip();
    }

    private void ImageViewport_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateClip();
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
