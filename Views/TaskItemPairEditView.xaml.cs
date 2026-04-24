using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class TaskItemPairEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskItemPairViewModel),
        typeof(TaskItemPairEditView),
        new PropertyMetadata(null));

    public TaskItemPairViewModel? ViewModel
    {
        get => (TaskItemPairViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler<int>? ImageReplaceRequested;

    public TaskItemPairEditView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += TaskItemPairEditView_Loaded;
    }

    private void TaskItemPairEditView_Loaded(object sender, RoutedEventArgs e)
    {
        TopItemEditor.SetFocus();
    }

    private void TopItemEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, 0); // 0 = Top item
    }

    private void BottomItemEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, 1); // 1 = Bottom item
    }
}
