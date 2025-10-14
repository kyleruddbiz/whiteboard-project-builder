using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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
        InitializeComponent();
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
            var viewbox = new Viewbox()
            {
                Stretch = Stretch.Uniform,
            };

            int row = i / 2;
            int col = i % 2;

            //int leftMargin = (col == 1) ? -1 : 0;
            int topMargin = (row == 1) ? -4 : 0;

            int leftMargin = 0;
            //int topMargin = 0;

            viewbox.Margin = new Thickness(leftMargin, topMargin, 0, 0);
            viewbox.Child = new ProjectItemView
            {
                ViewModel = Items[i]
            };

            Grid.SetRow(viewbox, row);
            Grid.SetColumn(viewbox, col);

            RootGrid.Children.Add(viewbox);
        }
    }
}
