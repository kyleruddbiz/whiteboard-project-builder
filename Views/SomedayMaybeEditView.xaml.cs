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
        this.InitializeComponent();
        this.DataContext = this;
    }

    private void ImageEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}
