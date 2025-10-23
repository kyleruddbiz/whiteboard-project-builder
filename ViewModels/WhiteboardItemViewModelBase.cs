using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public abstract partial class WhiteboardItemViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool isArchived;

    public DateTime CreatedDate { get; protected set; } = DateTime.Today;

    /// <summary>
    /// Raised when any property changes to trigger autosave.
    /// </summary>
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

    /// <summary>
    /// Gets the type of this whiteboard item.
    /// </summary>
    public abstract WhiteboardItemType GetItemType();

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public abstract IWhiteboardItem ToModel();

    /// <summary>
    /// Helper property for conditional rendering in XAML.
    /// </summary>
    public bool IsProjectItem => GetItemType() == WhiteboardItemType.Project;

    /// <summary>
    /// Helper property for conditional rendering in XAML.
    /// </summary>
    public bool IsSomedayMaybePair => GetItemType() == WhiteboardItemType.SomedayMaybe;
}
