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

    private string GetDateIsTodayPrefix(DateTime? date)
    {
        return date?.Date == DateTime.Now.Date
            ? "TODAY: "
            : string.Empty;
    }
}