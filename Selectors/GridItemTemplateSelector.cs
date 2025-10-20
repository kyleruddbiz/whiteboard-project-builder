using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Selectors;

public class GridItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AddButtonTemplate { get; set; }
    public DataTemplate? SelectorTemplate { get; set; }
    public DataTemplate? ProjectTemplate { get; set; }
    public DataTemplate? GoalTemplate { get; set; }
    public DataTemplate? InspirationTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is not GridItemWrapper wrapper)
            return null;

        return wrapper.GridItemType switch
        {
            GridItemType.AddButton => AddButtonTemplate,
            GridItemType.Selector => SelectorTemplate,
            GridItemType.WhiteboardItem => wrapper.WhiteboardItemType switch
            {
                WhiteboardItemType.Project => ProjectTemplate,
                WhiteboardItemType.Goal => GoalTemplate,
                WhiteboardItemType.Inspiration => InspirationTemplate,
                _ => null
            },
            _ => null
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
