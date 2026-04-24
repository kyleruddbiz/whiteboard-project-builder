using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class TaskSlotView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(TaskSlotViewModel),
        typeof(TaskSlotView),
        new PropertyMetadata(null));

    public TaskSlotViewModel? ViewModel
    {
        get => (TaskSlotViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public TaskSlotView()
    {
        InitializeComponent();
        DataContext = this;
    }
}
