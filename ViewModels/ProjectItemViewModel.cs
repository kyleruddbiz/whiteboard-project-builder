using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

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

    /// <summary>
    /// Raised when any property changes to trigger autosave.
    /// </summary>
    public event EventHandler? DataChanged;

    partial void OnTitleChanged(string value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSubtitleChanged(string? value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnImageChanged(string value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSizeChanged(ProjectSize value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(SizeIcon));
    }

    partial void OnValueChanged(ProjectValue value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(ValueIcon));
    }

    partial void OnDueDateChanged(DateTime? value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public ProjectItem ToModel()
    {
        return new ProjectItem
        {
            Title = Title,
            Subtitle = Subtitle,
            Image = Image,
            Size = Size,
            Value = Value,
            DueDate = DueDate
        };
    }

    /// <summary>
    /// Creates a ViewModel from a Model.
    /// </summary>
    public static ProjectItemViewModel FromModel(ProjectItem model)
    {
        return new ProjectItemViewModel
        {
            Title = model.Title,
            Subtitle = model.Subtitle,
            Image = model.Image,
            Size = model.Size,
            Value = model.Value,
            DueDate = model.DueDate
        };
    }
}
