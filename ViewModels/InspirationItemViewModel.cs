using CommunityToolkit.Mvvm.ComponentModel;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class InspirationItemViewModel : WhiteboardItemViewModelBase
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string image = string.Empty;

    partial void OnTextChanged(string value)
    {
        RaiseDataChanged();
    }

    partial void OnImageChanged(string value)
    {
        RaiseDataChanged();
    }
}
