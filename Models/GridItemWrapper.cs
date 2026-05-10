using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Models;

public partial class GridItemWrapper : ObservableObject
{
    [ObservableProperty]
    private GridItemType gridItemType;

    [ObservableProperty]
    private WhiteboardItemSize layoutSize;

    public object? Content
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(WhiteboardItem));
                OnPropertyChanged(nameof(ProjectItem));
                OnPropertyChanged(nameof(TaskItem));
            }
        }
    }

    public ProjectItemViewModel? ProjectItem => Content as ProjectItemViewModel;
    public TaskItemViewModel? TaskItem => Content as TaskItemViewModel;
    public WhiteboardItemViewModelBase? WhiteboardItem => Content as WhiteboardItemViewModelBase;

    public double Width => WhiteboardItemSizes.WidthOf(LayoutSize);
    public double Height => WhiteboardItemSizes.HeightOf(LayoutSize);
}
