using CommunityToolkit.Mvvm.ComponentModel;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class GoalItemViewModel : WhiteboardItemViewModelBase
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? subtitle;

    [ObservableProperty]
    private string image = string.Empty;

    partial void OnTitleChanged(string value)
    {
        RaiseDataChanged();
    }

    partial void OnSubtitleChanged(string? value)
    {
        RaiseDataChanged();
    }

    partial void OnImageChanged(string value)
    {
        RaiseDataChanged();
    }
}
