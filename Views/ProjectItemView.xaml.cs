using System.Windows.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ProjectItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProjectItemViewModel),
            typeof(ProjectItemView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
        nameof(EditCommand),
        typeof(ICommand),
        typeof(ProjectItemView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ImageReplaceCommandProperty = DependencyProperty.Register(
        nameof(ImageReplaceCommand),
        typeof(ICommand),
        typeof(ProjectItemView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ReactivateCommandProperty = DependencyProperty.Register(
        nameof(ReactivateCommand),
        typeof(ICommand),
        typeof(ProjectItemView),
        new PropertyMetadata(null));

    public ProjectItemViewModel? ViewModel
    {
        get => (ProjectItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public ICommand? ImageReplaceCommand
    {
        get => (ICommand?)GetValue(ImageReplaceCommandProperty);
        set => SetValue(ImageReplaceCommandProperty, value);
    }

    public ICommand? ReactivateCommand
    {
        get => (ICommand?)GetValue(ReactivateCommandProperty);
        set => SetValue(ReactivateCommandProperty, value);
    }

    public ProjectItemView()
    {
        InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (e.NewValue is ProjectItemViewModel vm)
            {
                ViewModel = vm;
            }
        };
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
        if (ViewModel != null && EditCommand?.CanExecute(ViewModel) == true)
        {
            EditCommand.Execute(ViewModel);
        }
    }

    private void ProjectItemEditView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        if (ImageReplaceCommand?.CanExecute(XamlRoot) == true)
        {
            ImageReplaceCommand.Execute(XamlRoot);
        }
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
        if (ViewModel != null && ReactivateCommand?.CanExecute(ViewModel) == true)
        {
            ReactivateCommand.Execute(ViewModel);
        }
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
