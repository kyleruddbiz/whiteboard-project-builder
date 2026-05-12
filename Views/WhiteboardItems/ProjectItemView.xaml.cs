using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WhiteboardProjectBuilder.ViewModels.WhiteboardItems;

namespace WhiteboardProjectBuilder.Views.WhiteboardItems;

public sealed partial class ProjectItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProjectItemViewModel),
            typeof(ProjectItemView),
            new PropertyMetadata(null));

    public ProjectItemViewModel ViewModel
    {
        get => (ProjectItemViewModel)GetValue(ViewModelProperty)!;
        set => SetValue(ViewModelProperty, value);
    }

    public ProjectItemView()
    {
        InitializeComponent();
    }

    private void Overlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border overlay && overlay.Child is Border indicator)
        {
            AnimateHoverIndicator(indicator, 0.3);
        }
    }

    private void Overlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border overlay && overlay.Child is Border indicator)
        {
            AnimateHoverIndicator(indicator, 0);
        }
    }

    private void Overlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.EnterEditModeCommand.Execute(null);
    }

    private void ProjectItemEditView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        ViewModel.ReplaceImageCommand.Execute(XamlRoot);
    }

    private void ReactivateOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border overlay && overlay.Child is Border indicator)
        {
            AnimateHoverIndicator(indicator, 0.3);
        }
    }

    private void ReactivateOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border overlay && overlay.Child is Border indicator)
        {
            AnimateHoverIndicator(indicator, 0);
        }
    }

    private void ReactivateOverlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.ReactivateCommand.Execute(null);
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
