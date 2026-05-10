using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Selectors;

public class WhiteboardItemBodyTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ProjectTemplate { get; set; }
    public DataTemplate? TaskItemTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            ProjectItemViewModel => ProjectTemplate,
            TaskItemViewModel => TaskItemTemplate,
            _ => null
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
