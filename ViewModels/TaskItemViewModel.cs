using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class TaskItemViewModel : WhiteboardItemViewModelBase
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

    public string CreatedDateDisplay => CreatedDate.ToShortDateString();
    public bool IsUsingTemporaryImage => Image.StartsWith("Assets/Backgrounds/Examples");
    public bool HasError => string.IsNullOrWhiteSpace(Title) || IsUsingTemporaryImage;

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

    public override WhiteboardItemType GetItemType() => WhiteboardItemType.TaskItem;

    public override TaskItem ToModel()
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

        return new TaskItem
        {
            Title = Title,
            Subtitle = Subtitle,
            Image = Image,
            Transform = transform,
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
        };
    }

    public static TaskItemViewModel FromModel(TaskItem model)
    {
        return new TaskItemViewModel
        {
            Title = model.Title,
            Subtitle = model.Subtitle,
            Image = model.Image,
            ImageOffsetX = model.Transform?.OffsetX ?? 0,
            ImageOffsetY = model.Transform?.OffsetY ?? 0,
            ImageZoomFactor = model.Transform?.ZoomFactor ?? 1.0,
            CreatedDate = model.CreatedDate,
            IsArchived = model.IsArchived
        };
    }
}
