using Microsoft.UI.Xaml.Input;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class TaskItemPairView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskItemPairViewModel),
        typeof(TaskItemPairView),
        new PropertyMetadata(null));

    public TaskItemPairViewModel? ViewModel
    {
        get => (TaskItemPairViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler? EditRequested;
    public event EventHandler<int>? ImageReplaceRequested;
    public event EventHandler? ReactivateRequested;

    public TaskItemPairView()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Overlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        HoverIndicator.Opacity = 0.15;
    }

    private void Overlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        HoverIndicator.Opacity = 0;
    }

    private void Overlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ReactivateOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ReactivateHoverIndicator.Opacity = 0.3;
    }

    private void ReactivateOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ReactivateHoverIndicator.Opacity = 0;
    }

    private void ReactivateOverlay_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ReactivateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void TaskItemPairEditView_ImageReplaceRequested(object sender, int itemIndex)
    {
        ImageReplaceRequested?.Invoke(this, itemIndex);
    }
}
