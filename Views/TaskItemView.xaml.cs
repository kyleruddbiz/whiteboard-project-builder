using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class TaskItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskItemViewModel),
        typeof(TaskItemView),
        new PropertyMetadata(null));

    public TaskItemViewModel? ViewModel
    {
        get => (TaskItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public TaskItemView()
    {
        InitializeComponent();
        DataContext = this;
    }
}
