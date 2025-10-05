using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Models;

public class GridItemWrapper
{
    public ProjectItemViewModel? ProjectItem { get; init; }
    public bool IsAddButton { get; init; }
}
