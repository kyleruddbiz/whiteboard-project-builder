using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class WhiteboardItemSelectorViewModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    public event EventHandler<WhiteboardItemType>? ItemTypeSelected;
    public event EventHandler? CancelRequested;

    [RelayCommand]
    private void SelectProjectItem()
    {
        ItemTypeSelected?.Invoke(this, WhiteboardItemType.Project);
    }

    [RelayCommand]
    private void SelectSomedayMaybeItem()
    {
        ItemTypeSelected?.Invoke(this, WhiteboardItemType.SomedayMaybe);
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
