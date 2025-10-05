using Microsoft.UI.Xaml;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class GoalItemView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(GoalItemViewModel),
            typeof(GoalItemView),
            new PropertyMetadata(null));

    public GoalItemViewModel? ViewModel
    {
        get => (GoalItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public GoalItemView()
    {
        this.InitializeComponent();
        this.DataContext = this;
    }
}
