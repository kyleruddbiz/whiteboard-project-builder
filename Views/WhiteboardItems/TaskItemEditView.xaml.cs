using WhiteboardProjectBuilder.ViewModels.WhiteboardItems;

namespace WhiteboardProjectBuilder.Views.WhiteboardItems;

public sealed partial class TaskItemEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskItemViewModel),
        typeof(TaskItemEditView),
        new PropertyMetadata(null));

    public TaskItemViewModel? ViewModel
    {
        get => (TaskItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler? ImageReplaceRequested;

    public TaskItemEditView()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void ImageEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}
