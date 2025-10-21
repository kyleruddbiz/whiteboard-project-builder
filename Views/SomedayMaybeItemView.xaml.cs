using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class SomedayMaybeItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(SomedayMaybeViewModel),
        typeof(SomedayMaybeItemView),
        new PropertyMetadata(null));

    public SomedayMaybeViewModel? ViewModel
    {
        get => (SomedayMaybeViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public SomedayMaybeItemView()
    {
        InitializeComponent();
        DataContext = this;
    }
}
