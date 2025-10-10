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

    public ProjectItemViewModel? ViewModel
    {
        get => (ProjectItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler<ProjectItemViewModel>? EditRequested;
    public event EventHandler? ImageReplaceRequested;

    public ProjectItemView()
    {
        this.InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (e.NewValue is ProjectItemViewModel vm)
            {
                ViewModel = vm;
            }
        };
    }

    public bool Not(bool value) => !value;

    private void Overlay_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (this.FindName("HoverIndicator") is Border indicator)
        {
            var animation = new DoubleAnimation
            {
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(animation, indicator);
            Storyboard.SetTargetProperty(animation, "Opacity");
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

    private void Overlay_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (this.FindName("HoverIndicator") is Border indicator)
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(animation, indicator);
            Storyboard.SetTargetProperty(animation, "Opacity");
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

    private void Overlay_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            EditRequested?.Invoke(this, ViewModel);
        }
    }

    private void ProjectItemEditView_ImageReplaceRequested(object? sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}