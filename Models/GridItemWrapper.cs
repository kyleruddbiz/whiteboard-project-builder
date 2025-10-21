using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Models;

public partial class GridItemWrapper : ObservableObject
{
    [ObservableProperty]
    private GridItemType gridItemType;

    [ObservableProperty]
    private WhiteboardItemType? whiteboardItemType;

    private object? content;
    public object? Content
    {
        get => content;
        set
        {
            if (SetProperty(ref content, value))
            {
                switch (GridItemType)
                {
                    case GridItemType.WhiteboardItem:
                        OnPropertyChanged(nameof(WhiteboardItem));
                        switch (WhiteboardItemType)
                        {
                            case Enums.WhiteboardItemType.Project:
                                OnPropertyChanged(nameof(ProjectItem));
                                break;
                            case Enums.WhiteboardItemType.SomedayMaybe:
                                OnPropertyChanged(nameof(SomedayMaybePair));
                                break;
                            case Enums.WhiteboardItemType.Goal:
                                OnPropertyChanged(nameof(GoalItem));
                                break;
                            case Enums.WhiteboardItemType.Inspiration:
                                OnPropertyChanged(nameof(InspirationItem));
                                break;
                        }
                        break;
                    case GridItemType.Selector:
                        OnPropertyChanged(nameof(Selector));
                        break;
                }
            }
        }
    }

    // Convenience accessors for XAML x:Bind (returns concrete types)
    public ProjectItemViewModel? ProjectItem => Content as ProjectItemViewModel;
    public SomedayMaybePairViewModel? SomedayMaybePair => Content as SomedayMaybePairViewModel;
    public GoalItemViewModel? GoalItem => Content as GoalItemViewModel;
    public InspirationItemViewModel? InspirationItem => Content as InspirationItemViewModel;
    public WhiteboardItemSelectorViewModel? Selector => Content as WhiteboardItemSelectorViewModel;
    public WhiteboardItemViewModelBase? WhiteboardItem => Content as WhiteboardItemViewModelBase;
}
