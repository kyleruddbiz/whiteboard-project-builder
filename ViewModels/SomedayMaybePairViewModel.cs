using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class SomedayMaybePairViewModel : WhiteboardItemViewModelBase
{
    private readonly ImageStorageService imageStorageService;
    private readonly ImageTransformService imageTransformService;
    private readonly ImageDimensionService imageDimensionService;

    [ObservableProperty]
    private SomedayMaybeViewModel topItem = null!;

    [ObservableProperty]
    private SomedayMaybeViewModel? bottomItem;

    public SomedayMaybePairViewModel(ImageStorageService imageStorageService, ImageTransformService imageTransformService, ImageDimensionService imageDimensionService)
    {
        this.imageStorageService = imageStorageService;
        this.imageTransformService = imageTransformService;
        this.imageDimensionService = imageDimensionService;
    }

    public bool HasBottomItem => BottomItem != null;
    public bool ShowAddBottomButton => !HasBottomItem && IsEditing;

    partial void OnTopItemChanged(SomedayMaybeViewModel? oldValue, SomedayMaybeViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnItemDataChanged;
        }

        if (newValue != null)
        {
            newValue.DataChanged += OnItemDataChanged;
        }

        OnPropertyChanged(nameof(HasBottomItem));
        OnPropertyChanged(nameof(ShowAddBottomButton));
        RaiseDataChanged();
    }

    partial void OnBottomItemChanged(SomedayMaybeViewModel? oldValue, SomedayMaybeViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnItemDataChanged;
        }

        if (newValue != null)
        {
            newValue.DataChanged += OnItemDataChanged;
        }

        OnPropertyChanged(nameof(HasBottomItem));
        OnPropertyChanged(nameof(ShowAddBottomButton));
        RaiseDataChanged();
    }

    private void OnItemDataChanged(object? sender, EventArgs e)
    {
        RaiseDataChanged();
    }

    [RelayCommand]
    private async Task AddBottomItemAsync()
    {
        if (BottomItem == null)
        {
            string imagePath = await imageStorageService.GetRandomDefaultImagePathAsync();

            BottomItem = new SomedayMaybeViewModel
            {
                Image = imagePath,
                CreatedDate = DateTime.Today
            };

            await ApplyUniformToFillTransformAsync(BottomItem, imagePath);
        }
    }

    [RelayCommand]
    private void RemoveBottomItem()
    {
        BottomItem = null;
    }

    /// <summary>
    /// Applies UniformToFill transform to a someday maybe item based on its image URI.
    /// Used for default background images loaded from ms-appx:// URIs.
    /// </summary>
    private async Task ApplyUniformToFillTransformAsync(SomedayMaybeViewModel item, string imageUri)
    {
        try
        {
            var (width, height) = await imageDimensionService.GetImageDimensionsAsync(imageUri);
            var (scale, offsetX, offsetY) = imageTransformService.CalculateUniformToFillTransform(width, height);

            item.ImageZoomFactor = scale;
            item.ImageOffsetX = offsetX;
            item.ImageOffsetY = offsetY;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply UniformToFill transform: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the type of this whiteboard item.
    /// </summary>
    public override WhiteboardItemType GetItemType() => WhiteboardItemType.SomedayMaybe;

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public override IWhiteboardItem ToModel()
    {
        return new SomedayMaybePair
        {
            TopItem = TopItem.ToModel(),
            BottomItem = BottomItem?.ToModel(),
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
        };
    }

    /// <summary>
    /// Populates this ViewModel from a Model.
    /// </summary>
    public void LoadFromModel(SomedayMaybePair model)
    {
        TopItem = SomedayMaybeViewModel.FromModel(model.TopItem);
        BottomItem = model.BottomItem != null ? SomedayMaybeViewModel.FromModel(model.BottomItem) : null;
        CreatedDate = model.CreatedDate;
        IsArchived = model.IsArchived;
    }
}
