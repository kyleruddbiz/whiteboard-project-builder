using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class ProjectItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? subtitle;

    [ObservableProperty]
    private string image = string.Empty;

    [ObservableProperty]
    private ProjectSize size;

    [ObservableProperty]
    private ProjectValue value;

    [ObservableProperty]
    private DateTime? dueDate;

    public string SizeIcon => Size.ToIcon();
    public string ValueIcon => Value.ToIcon();
}
