using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Selectors;

public class GridItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AddProjectButtonTemplate { get; set; }
    public DataTemplate? AddTaskButtonTemplate { get; set; }
    public DataTemplate? ProjectTemplate { get; set; }
    public DataTemplate? TaskItemTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is not GridItemWrapper wrapper)
            return null;

        return wrapper.GridItemType switch
        {
            GridItemType.AddButton => wrapper.WhiteboardItemType switch
            {
                WhiteboardItemType.Project => AddProjectButtonTemplate,
                WhiteboardItemType.TaskItem => AddTaskButtonTemplate,
                _ => null
            },
            GridItemType.WhiteboardItem => wrapper.WhiteboardItemType switch
            {
                WhiteboardItemType.Project => ProjectTemplate,
                WhiteboardItemType.TaskItem => TaskItemTemplate,
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
