using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Models;

public partial class GridItemWrapper : ObservableObject
{
    [ObservableProperty]
    private ProjectItemViewModel? projectItem;

    [ObservableProperty]
    private bool isAddButton;
}
