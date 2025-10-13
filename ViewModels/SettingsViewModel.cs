using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool showArchived;

    [ObservableProperty]
    private bool isSortDescending;

    [ObservableProperty]
    private bool isDeveloperMode;

    /// <summary>
    /// Raised when any property changes to trigger autosave.
    /// </summary>
    public event EventHandler? DataChanged;

    partial void OnShowArchivedChanged(bool value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnIsSortDescendingChanged(bool value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnIsDeveloperModeChanged(bool value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public Settings ToModel()
    {
        return new Settings
        {
            ShowArchived = ShowArchived,
            IsSortDescending = IsSortDescending,
            IsDeveloperMode = IsDeveloperMode
        };
    }

    /// <summary>
    /// Creates a ViewModel from a Model.
    /// </summary>
    public static SettingsViewModel FromModel(Settings model)
    {
        return new SettingsViewModel
        {
            ShowArchived = model.ShowArchived,
            IsSortDescending = model.IsSortDescending,
            IsDeveloperMode = model.IsDeveloperMode
        };
    }
}
