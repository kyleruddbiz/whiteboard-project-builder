using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class WhiteboardItemSelectorView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(WhiteboardItemSelectorViewModel),
            typeof(WhiteboardItemSelectorView),
            new PropertyMetadata(null));

    public WhiteboardItemSelectorViewModel? ViewModel
    {
        get => (WhiteboardItemSelectorViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public WhiteboardItemSelectorView()
    {
        InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (e.NewValue is WhiteboardItemSelectorViewModel vm)
            {
                ViewModel = vm;
            }
        };
    }
}
