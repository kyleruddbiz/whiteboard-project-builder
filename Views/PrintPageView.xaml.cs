using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class PrintPageView : UserControl
{
    public static readonly DependencyProperty LayoutProperty =
        DependencyProperty.Register(
            nameof(Layout),
            typeof(PrintPageLayout),
            typeof(PrintPageView),
            new PropertyMetadata(null, OnLayoutChanged));

    public PrintPageLayout? Layout
    {
        get => (PrintPageLayout?)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public PrintPageView()
    {
        InitializeComponent();
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PrintPageView view)
        {
            view.PopulateGrid();
        }
    }

    private void PopulateGrid()
    {
        RootGrid.Children.Clear();

        if (Layout == null)
        {
            return;
        }

        foreach (var placement in Layout.Placements)
        {
            UserControl itemView = placement.Slot switch
            {
                ProjectItemViewModel projectVm => new ProjectItemView { ViewModel = projectVm },
                TaskItemViewModel taskVm => new TaskItemView { ViewModel = taskVm },
                FullImageItemViewModel fullImageVm => new FullImageItemView { ViewModel = fullImageVm },
                _ => throw new NotSupportedException($"Unsupported print slot type: {placement.Slot.GetType().Name}")
            };

            itemView.Width = WhiteboardItemSizes.WidthOf(placement.Slot.LayoutSize);
            itemView.Height = WhiteboardItemSizes.HeightOf(placement.Slot.LayoutSize);

            var viewbox = new Viewbox
            {
                Stretch = Stretch.Fill,
                Child = itemView,
            };

            Grid.SetRow(viewbox, placement.Row);
            Grid.SetColumn(viewbox, placement.Col);
            Grid.SetRowSpan(viewbox, placement.RowSpan);
            Grid.SetColumnSpan(viewbox, placement.ColSpan);

            RootGrid.Children.Add(viewbox);
        }
    }
}
