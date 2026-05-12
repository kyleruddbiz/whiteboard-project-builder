using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels.WhiteboardItems;

public abstract partial class WhiteboardItemViewModelBase(WhiteboardItemWorkspaceViewModel workspace) : ObservableObject
{
    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool isArchived;

    public DateTime CreatedDate { get; protected set; } = DateTime.Today;

    public event EventHandler? DataChanged;

    protected void RaiseDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnIsArchivedChanged(bool value)
    {
        RaiseDataChanged();
    }

    [RelayCommand]
    private void CancelEditing()
    {
        if (IsEditing)
        {
            IsEditing = false;
        }
    }

    [RelayCommand]
    private void EnterEditMode() => workspace.EnterEditMode(this);

    [RelayCommand]
    private void Reactivate() => workspace.ReactivateItem(this);

    [RelayCommand]
    private Task ReplaceImageAsync(XamlRoot xamlRoot) => workspace.ReplaceImageAsync(this, xamlRoot);

    public abstract WhiteboardItemType ItemType { get; }

    public abstract WhiteboardItemSize LayoutSize { get; }

    public abstract IWhiteboardItem ToModel();
}
