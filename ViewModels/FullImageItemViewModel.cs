using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class FullImageItemViewModel : WhiteboardItemViewModelBase, IPrintSlot, ISingleImageItem
{
    private const double BorderThickness = 2;

    public FullImageItemViewModel(WhiteboardItemWorkspaceViewModel workspace, WhiteboardItemSize size) : base(workspace)
    {
        LayoutSize = size;
        ImageAreaWidth = WhiteboardItemSizes.WidthOf(size);
        ImageAreaHeight = WhiteboardItemSizes.HeightOf(size);
        ImageClipWidth = ImageAreaWidth - 2 * BorderThickness;
        ImageClipHeight = ImageAreaHeight - 2 * BorderThickness;
    }

    public double ImageAreaWidth { get; }
    public double ImageAreaHeight { get; }
    public double ImageClipWidth { get; }
    public double ImageClipHeight { get; }

    [ObservableProperty]
    private string image = string.Empty;

    [ObservableProperty]
    private double imageOffsetX = 0;

    [ObservableProperty]
    private double imageOffsetY = 0;

    [ObservableProperty]
    private double imageZoomFactor = 1.0;

    partial void OnImageChanged(string value) => RaiseDataChanged();
    partial void OnImageOffsetXChanged(double value) => RaiseDataChanged();
    partial void OnImageOffsetYChanged(double value) => RaiseDataChanged();
    partial void OnImageZoomFactorChanged(double value) => RaiseDataChanged();

    public override WhiteboardItemType ItemType => WhiteboardItemType.FullImage;

    public override WhiteboardItemSize LayoutSize { get; }

    public override FullImageItem ToModel()
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

        return new FullImageItem
        {
            Image = Image,
            Transform = transform,
            Size = LayoutSize,
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
        };
    }

    public static FullImageItemViewModel FromModel(FullImageItem model, WhiteboardItemWorkspaceViewModel workspace)
    {
        return new FullImageItemViewModel(workspace, model.Size)
        {
            Image = model.Image,
            ImageOffsetX = model.Transform?.OffsetX ?? 0,
            ImageOffsetY = model.Transform?.OffsetY ?? 0,
            ImageZoomFactor = Math.Max(model.Transform?.ZoomFactor ?? 1.0, ImageTransformService.MinZoomFactor),
            CreatedDate = model.CreatedDate,
            IsArchived = model.IsArchived
        };
    }
}
