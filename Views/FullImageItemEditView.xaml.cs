using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class FullImageItemEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(FullImageItemViewModel),
        typeof(FullImageItemEditView),
        new PropertyMetadata(null));

    public FullImageItemViewModel ViewModel
    {
        get => (FullImageItemViewModel)GetValue(ViewModelProperty)!;
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler? ImageReplaceRequested;

    public FullImageItemEditView()
    {
        InitializeComponent();

        Loaded += FullImageItemEditView_Loaded;
    }

    private void FullImageItemEditView_Loaded(object sender, RoutedEventArgs e)
    {
        MainImageEditor.SetFocus();
    }

    private void ImageEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, EventArgs.Empty);
    }
}
