using CommunityToolkit.Mvvm.ComponentModel;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class GoalItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? subtitle;

    [ObservableProperty]
    private string image = string.Empty;
}
