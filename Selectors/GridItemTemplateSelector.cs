using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.Selectors;

public class GridItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ProjectTemplate { get; set; }
    public DataTemplate? AddButtonTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item is GridItemWrapper wrapper
            ? wrapper.IsAddButton ? AddButtonTemplate : ProjectTemplate
            : null;
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
