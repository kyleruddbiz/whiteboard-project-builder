using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class ProjectItemViewModel : WhiteboardItemViewModelBase
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
    private double imageZoomFactor = 1.0;

    [ObservableProperty]
    private ProjectSize size;

    [ObservableProperty]
    private ProjectValue value;

    [ObservableProperty]
    private DateTime? dueDate;

    public string SizeIcon => Size.ToIcon();
    public string ValueIcon => Value.ToIcon();
    public Brush SizeValueBorderBrush
    {
        get
        {
            var category = WinCategoryExtensions.GetWinCategory(Size, Value);
            var color = category.GetBorderColor();
            return new SolidColorBrush(color);
        }
    }
    public string DateDisplay => DueDate?.ToShortDateString() ?? string.Empty;
    public string DatePrefix => DueDate?.Date == DateTime.Now.Date ? "TODAY: " : string.Empty;
    public string CreatedDateDisplay => CreatedDate.ToShortDateString();
    public bool IsUsingTemporaryImage => Image.StartsWith("Assets/Backgrounds/Examples");
    public bool HasError => string.IsNullOrWhiteSpace(Title) || IsUsingTemporaryImage;

    public List<ProjectSize> SizeOptions { get; } = [.. Enum.GetValues<ProjectSize>().Cast<ProjectSize>()];
    public List<ProjectValue> ValueOptions { get; } = [.. Enum.GetValues<ProjectValue>().Cast<ProjectValue>()];

    public List<IconOption<ProjectSize>> SizeIconOptions { get; } = IconOption<ProjectSize>.Create(s => s.ToIcon());
    public List<IconOption<ProjectValue>> ValueIconOptions { get; } = IconOption<ProjectValue>.Create(v => v.ToIcon());

    partial void OnTitleChanged(string value)
    {
        RaiseDataChanged();
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnSubtitleChanged(string? value)
    {
        RaiseDataChanged();
    }

    partial void OnImageChanged(string value)
    {
        RaiseDataChanged();
        OnPropertyChanged(nameof(IsUsingTemporaryImage));
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnImageOffsetXChanged(double value)
    {
        RaiseDataChanged();
    }

    partial void OnImageOffsetYChanged(double value)
    {
        RaiseDataChanged();
    }

    partial void OnImageZoomFactorChanged(double value)
    {
        RaiseDataChanged();
    }

    partial void OnSizeChanged(ProjectSize value)
    {
        RaiseDataChanged();
        OnPropertyChanged(nameof(SizeIcon));
        OnPropertyChanged(nameof(SizeValueBorderBrush));
    }

    partial void OnValueChanged(ProjectValue value)
    {
        RaiseDataChanged();
        OnPropertyChanged(nameof(ValueIcon));
        OnPropertyChanged(nameof(SizeValueBorderBrush));
    }

    partial void OnDueDateChanged(DateTime? value)
    {
        RaiseDataChanged();
        OnPropertyChanged(nameof(DateDisplay));
        OnPropertyChanged(nameof(DatePrefix));
    }

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public ProjectItem ToModel()
    {
        ImageTransform? transform = null;
        if (ImageOffsetX != 0 || ImageOffsetY != 0 || ImageZoomFactor != 1.0)
        {
            transform = new ImageTransform
            {
                OffsetX = ImageOffsetX,
                OffsetY = ImageOffsetY,
                ZoomFactor = ImageZoomFactor
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
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
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
            ImageZoomFactor = model.Transform?.ZoomFactor ?? 1.0,
            Size = model.Size,
            Value = model.Value,
            DueDate = model.DueDate,
            CreatedDate = model.CreatedDate,
            IsArchived = model.IsArchived
        };
    }
}
