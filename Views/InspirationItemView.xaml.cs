using Microsoft.UI.Xaml;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class InspirationItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(InspirationItemViewModel),
            typeof(InspirationItemView),
            new PropertyMetadata(null));

    public InspirationItemViewModel? ViewModel
    {
        get => (InspirationItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public InspirationItemView()
    {
        InitializeComponent();
        DataContext = this;
    }
}
