using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class SomedayMaybePairEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(SomedayMaybePairViewModel),
        typeof(SomedayMaybePairEditView),
        new PropertyMetadata(null));

    public SomedayMaybePairViewModel? ViewModel
    {
        get => (SomedayMaybePairViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler<int>? ImageReplaceRequested;

    public SomedayMaybePairEditView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += SomedayMaybePairEditView_Loaded;
    }

    private void SomedayMaybePairEditView_Loaded(object sender, RoutedEventArgs e)
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
