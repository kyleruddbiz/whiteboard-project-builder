using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Selectors;

public class GridItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AddButtonTemplate { get; set; }
    public DataTemplate? WhiteboardItemTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is not GridItemWrapper wrapper)
            return null;

        return wrapper.GridItemType switch
        {
            GridItemType.AddButton => AddButtonTemplate,
            GridItemType.WhiteboardItem => WhiteboardItemTemplate,
            _ => null
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
