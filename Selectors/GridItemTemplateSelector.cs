using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Selectors;

public class GridItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AddButtonTemplate { get; set; }
    public DataTemplate? ProjectMediumTemplate { get; set; }
    public DataTemplate? TaskItemSmallTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is not GridItemWrapper wrapper)
            return null;

        if (wrapper.GridItemType == GridItemType.AddButton)
        {
            return AddButtonTemplate;
        }

        return (wrapper.WhiteboardItemType, wrapper.LayoutSize) switch
        {
            (WhiteboardItemType.Project, WhiteboardItemSize.Medium) => ProjectMediumTemplate,
            (WhiteboardItemType.TaskItem, WhiteboardItemSize.Small) => TaskItemSmallTemplate,
            _ => null
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
