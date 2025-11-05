using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class SomedayMaybeEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(SomedayMaybeViewModel),
        typeof(SomedayMaybeEditView),
        new PropertyMetadata(null));

    public SomedayMaybeViewModel? ViewModel
    {
        get => (SomedayMaybeViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler? ImageReplaceRequested;

    public SomedayMaybeEditView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void SetFocus()
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
