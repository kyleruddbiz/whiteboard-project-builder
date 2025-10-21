using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class SomedayMaybeViewModel : ObservableObject
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

    public DateTime CreatedDate { get; set; } = DateTime.Today;

    public string CreatedDateDisplay => CreatedDate.ToShortDateString();
    public bool IsUsingTemporaryImage => Image.StartsWith("Assets/Backgrounds/Examples");
    public bool HasError => string.IsNullOrWhiteSpace(Title) || IsUsingTemporaryImage;

    /// <summary>
    /// Raised when any property changes to trigger autosave.
    /// </summary>
    public event EventHandler? DataChanged;

    protected void RaiseDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

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

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public SomedayMaybe ToModel()
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

        return new SomedayMaybe
        {
            Title = Title,
            Subtitle = Subtitle,
            Image = Image,
            Transform = transform,
            CreatedDate = CreatedDate
        };
    }

    /// <summary>
    /// Creates a ViewModel from a Model.
    /// </summary>
    public static SomedayMaybeViewModel FromModel(SomedayMaybe model)
    {
        return new SomedayMaybeViewModel
        {
            Title = model.Title,
            Subtitle = model.Subtitle,
            Image = model.Image,
            ImageOffsetX = model.Transform?.OffsetX ?? 0,
            ImageOffsetY = model.Transform?.OffsetY ?? 0,
            ImageZoomFactor = model.Transform?.ZoomFactor ?? 1.0,
            CreatedDate = model.CreatedDate
        };
    }
}
