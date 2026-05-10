using System.Windows.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class TaskItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskItemViewModel),
        typeof(TaskItemView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
        nameof(EditCommand),
        typeof(ICommand),
        typeof(TaskItemView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ImageReplaceCommandProperty = DependencyProperty.Register(
        nameof(ImageReplaceCommand),
        typeof(ICommand),
        typeof(TaskItemView),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ReactivateCommandProperty = DependencyProperty.Register(
        nameof(ReactivateCommand),
        typeof(ICommand),
        typeof(TaskItemView),
        new PropertyMetadata(null));

    public TaskItemViewModel? ViewModel
    {
        get => (TaskItemViewModel?)GetValue(ViewModelProperty);
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

    public TaskItemView()
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
        if (ViewModel != null && EditCommand?.CanExecute(ViewModel) == true)
        {
            EditCommand.Execute(ViewModel);
        }
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
        if (ViewModel != null && ReactivateCommand?.CanExecute(ViewModel) == true)
        {
            ReactivateCommand.Execute(ViewModel);
        }
    }

    private void TaskItemEditView_ImageReplaceRequested(object sender, EventArgs e)
    {
        if (ImageReplaceCommand?.CanExecute(XamlRoot) == true)
        {
            ImageReplaceCommand.Execute(XamlRoot);
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
