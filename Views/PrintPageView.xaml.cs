using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class PrintPageView : UserControl
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(List<IPrintSlot>),
            typeof(PrintPageView),
            new PropertyMetadata(null, OnItemsChanged));

    public List<IPrintSlot>? Items
    {
        get => (List<IPrintSlot>?)GetValue(ItemsProperty);
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

            int topMargin = (row == 1) ? -4 : 0;
            int leftMargin = 0;

            viewbox.Margin = new Thickness(leftMargin, topMargin, 0, 0);

            UserControl itemView = Items[i] switch
            {
                ProjectItemViewModel projectVm => new ProjectItemView { ViewModel = projectVm },
                TaskSlotViewModel slotVm => new TaskSlotView { ViewModel = slotVm },
                var slot => throw new NotSupportedException($"Unsupported print slot type: {slot.GetType().Name}")
            };

            viewbox.Child = itemView;

            Grid.SetRow(viewbox, row);
            Grid.SetColumn(viewbox, col);

            RootGrid.Children.Add(viewbox);
        }
    }
}
