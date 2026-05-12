using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WhiteboardProjectBuilder.ViewModels.WhiteboardItems;

namespace WhiteboardProjectBuilder.Views.WhiteboardItems;

public sealed partial class FullImageItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(FullImageItemViewModel),
            typeof(FullImageItemView),
            new PropertyMetadata(null));

    public FullImageItemViewModel ViewModel
    {
        get => (FullImageItemViewModel)GetValue(ViewModelProperty)!;
        set => SetValue(ViewModelProperty, value);
    }

    public FullImageItemView()
    {
        InitializeComponent();
    }

    private void Overlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverIndicator(HoverIndicator, 0.3);
    }

    private void Overlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverIndicator(HoverIndicator, 0);
    }

    private void Overlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.EnterEditModeCommand.Execute(null);
    }

    private void ReactivateOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverIndicator(ReactivateHoverIndicator, 0.3);
    }

    private void ReactivateOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverIndicator(ReactivateHoverIndicator, 0);
    }

    private void ReactivateOverlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.ReactivateCommand.Execute(null);
    }

    private void FullImageItemEditView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        ViewModel.ReplaceImageCommand.Execute(XamlRoot);
    }

    private static void AnimateHoverIndicator(UIElement target, double toOpacity)
    {
        var animation = new DoubleAnimation
        {
            To = toOpacity,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, "Opacity");
        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }
}
