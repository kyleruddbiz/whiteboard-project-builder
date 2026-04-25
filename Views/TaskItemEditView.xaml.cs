using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

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

        Loaded += TaskItemEditView_Loaded;
    }

    private void TaskItemEditView_Loaded(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel?.Title))
        {
            TitleTextBox.Focus(FocusState.Programmatic);
        }
        else
        {
            MainImageEditor.SetFocus();
        }
    }

    private void ImageEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}
