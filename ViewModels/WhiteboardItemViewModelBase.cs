using CommunityToolkit.Mvvm.ComponentModel;

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
}
