using CommunityToolkit.Mvvm.ComponentModel;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class InspirationItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string image = string.Empty;
}
