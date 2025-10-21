using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

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

    public override WhiteboardItemType GetItemType() => WhiteboardItemType.Inspiration;

    public override IWhiteboardItem ToModel()
    {
        throw new NotImplementedException("Inspiration items are not yet implemented");
    }
}
