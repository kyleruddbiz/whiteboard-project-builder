using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Helpers;
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
    private double imageOffsetX = 0;

    [ObservableProperty]
    private double imageOffsetY = 0;

    [ObservableProperty]
    private double imageScale = 1.0;

    [ObservableProperty]
    private ProjectSize size;

    [ObservableProperty]
    private ProjectValue value;

    [ObservableProperty]
    private DateTime? dueDate;

    [ObservableProperty]
    private bool isEditing;

    public DateTime CreatedDate { get; private set; } = DateTime.Today;

    public string SizeIcon => Size.ToIcon();
    public string ValueIcon => Value.ToIcon();
    public string DateDisplay => DueDate?.ToShortDateString() ?? string.Empty;
    public string DatePrefix => DueDate?.Date == DateTime.Now.Date ? "TODAY: " : string.Empty;
    public string CreatedDateDisplay => CreatedDate.ToShortDateString();

    public List<ProjectSize> SizeOptions { get; } = Enum.GetValues(typeof(ProjectSize)).Cast<ProjectSize>().ToList();
    public List<ProjectValue> ValueOptions { get; } = Enum.GetValues(typeof(ProjectValue)).Cast<ProjectValue>().ToList();

    public List<IconOption<ProjectSize>> SizeIconOptions { get; } = IconOption<ProjectSize>.Create(s => s.ToIcon());
    public List<IconOption<ProjectValue>> ValueIconOptions { get; } = IconOption<ProjectValue>.Create(v => v.ToIcon());

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

    partial void OnImageOffsetXChanged(double value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnImageOffsetYChanged(double value)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnImageScaleChanged(double value)
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
        OnPropertyChanged(nameof(DateDisplay));
        OnPropertyChanged(nameof(DatePrefix));
    }

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public ProjectItem ToModel()
    {
        ImageTransform? transform = null;
        if (ImageOffsetX != 0 || ImageOffsetY != 0 || ImageScale != 1.0)
        {
            transform = new ImageTransform
            {
                OffsetX = ImageOffsetX,
                OffsetY = ImageOffsetY,
                Scale = ImageScale
            };
        }

        return new ProjectItem
        {
            Title = Title,
            Subtitle = Subtitle,
            Image = Image,
            Transform = transform,
            Size = Size,
            Value = Value,
            DueDate = DueDate,
            CreatedDate = CreatedDate
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
            ImageOffsetX = model.Transform?.OffsetX ?? 0,
            ImageOffsetY = model.Transform?.OffsetY ?? 0,
            ImageScale = model.Transform?.Scale ?? 1.0,
            Size = model.Size,
            Value = model.Value,
            DueDate = model.DueDate,
            CreatedDate = model.CreatedDate
        };
    }
}