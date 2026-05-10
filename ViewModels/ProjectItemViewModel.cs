using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class ProjectItemViewModel : WhiteboardItemViewModelBase, IPrintSlot
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
    private ProjectSize projectSize;

    [ObservableProperty]
    private ProjectValue value;

    [ObservableProperty]
    private DateTime? dueDate;

    public string SizeIcon => ProjectSize.ToIcon();
    public string ValueIcon => Value.ToIcon();
    public Brush SizeValueBorderBrush
    {
        get
        {
            var category = WinCategoryExtensions.GetWinCategory(ProjectSize, Value);
            var color = category.GetBorderColor();
            return new SolidColorBrush(color);
        }
    }
    public string DateDisplay => DueDate?.ToShortDateString() ?? string.Empty;
    public string DatePrefix => DueDate?.Date == DateTime.Now.Date ? "TODAY: " : string.Empty;
    public string CreatedDateDisplay => CreatedDate.ToShortDateString();

    public List<ProjectSize> SizeOptions { get; } = [.. Enum.GetValues<ProjectSize>().Cast<ProjectSize>()];
    public List<ProjectValue> ValueOptions { get; } = [.. Enum.GetValues<ProjectValue>().Cast<ProjectValue>()];

    public List<IconOption<ProjectSize>> SizeIconOptions { get; } = IconOption<ProjectSize>.Create(s => s.ToIcon());
    public List<IconOption<ProjectValue>> ValueIconOptions { get; } = IconOption<ProjectValue>.Create(v => v.ToIcon());

    partial void OnTitleChanged(string value)
    {
        RaiseDataChanged();
    }

    partial void OnSubtitleChanged(string? value)
    {
        RaiseDataChanged();
    }

    partial void OnImageChanged(string value)
    {
        RaiseDataChanged();
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

    partial void OnProjectSizeChanged(ProjectSize value)
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

    public override WhiteboardItemType ItemType => WhiteboardItemType.Project;

    public override WhiteboardItemSize LayoutSize => WhiteboardItemSize.Medium;

    public override ProjectItem ToModel()
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
            Size = ProjectSize,
            Value = Value,
            DueDate = DueDate,
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
        };
    }

    public static ProjectItemViewModel FromModel(ProjectItem model)
    {
        return new ProjectItemViewModel
        {
            Title = model.Title,
            Subtitle = model.Subtitle,
            Image = model.Image,
            ImageOffsetX = model.Transform?.OffsetX ?? 0,
            ImageOffsetY = model.Transform?.OffsetY ?? 0,
            ImageZoomFactor = Math.Max(model.Transform?.ZoomFactor ?? 1.0, ImageTransformService.MinZoomFactor),
            ProjectSize = model.Size,
            Value = model.Value,
            DueDate = model.DueDate,
            CreatedDate = model.CreatedDate,
            IsArchived = model.IsArchived
        };
    }
}
