using Microsoft.UI.Xaml;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class PrintPageView : UserControl
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(List<ProjectItemViewModel>),
            typeof(PrintPageView),
            new PropertyMetadata(null, OnItemsChanged));

    public List<ProjectItemViewModel>? Items
    {
        get => (List<ProjectItemViewModel>?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public PrintPageView()
    {
        this.InitializeComponent();
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PrintPageView view)
        {
            view.PopulateGrid();
        }
    }

    private void PopulateGrid()
    {
        RootGrid.Children.Clear();

        if (Items == null || Items.Count == 0)
            return;

        for (int i = 0; i < Math.Min(Items.Count, 4); i++)
        {
            var projectItemView = new ProjectItemView
            {
                ViewModel = Items[i]
            };

            int row = i / 2;
            int col = i % 2;

            Grid.SetRow(projectItemView, row);
            Grid.SetColumn(projectItemView, col);

            RootGrid.Children.Add(projectItemView);
        }
    }
}
