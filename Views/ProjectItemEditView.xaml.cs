using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ProjectItemEditView : UserControl
{
    public event EventHandler? ImageReplaceRequested;

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProjectItemViewModel),
            typeof(ProjectItemEditView),
            new PropertyMetadata(null));

    public ProjectItemViewModel? ViewModel
    {
        get => (ProjectItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ProjectItemEditView()
    {
        InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (e.NewValue is ProjectItemViewModel vm)
            {
                ViewModel = vm;
            }
        };

        Loaded += ProjectItemEditView_Loaded;
    }

    private void ProjectItemEditView_Loaded(object sender, RoutedEventArgs e)
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

    private void ImageEditor_ImageReplaceRequested(object? sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}
