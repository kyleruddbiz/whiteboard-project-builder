using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.Services;

namespace WhiteboardProjectBuilder.ViewModels.WhiteboardItems;

public partial class FullImageItemViewModel : WhiteboardItemViewModelBase, IPrintSlot, ISingleImageItem
{
    // ImageAreaWidth/Height is the full cell footprint. The cell is inset by HostMargin
    // (MainPage's ContentControl Margin) before FullImageItemView renders, then by the
    // WhiteboardItem-level outer Border. ImageViewport's internal border is disabled here
    // (ShowImageBorder=False), so the clip area runs to the inside of that outer Border.
    // The editor mirrors the chain at the SAME logical clip size so pan/zoom values map 1:1
    // between edit and readonly; a Viewbox absorbs the small physical-vs-logical gap.
    private const double HostMargin = 10;
    private const double OuterBorder = 2;
    // Overscale guard: tell CalculateDefaultTransform the clip is slightly smaller than it
    // actually is so the default zoom always overshoots and the image fills (cropping a few
    // px) instead of letterboxing on aspect-mismatched images.
    private const double OverscaleGuard = 4;

    public FullImageItemViewModel(WhiteboardItemWorkspaceViewModel workspace, WhiteboardItemSize size) : base(workspace)
    {
        LayoutSize = size;
        ImageAreaWidth = WhiteboardItemSizes.WidthOf(size);
        ImageAreaHeight = WhiteboardItemSizes.HeightOf(size);
        ImageEditorWidth = ImageAreaWidth - 2 * (HostMargin + OuterBorder);
        ImageEditorHeight = ImageAreaHeight - 2 * (HostMargin + OuterBorder);
        ImageClipWidth = ImageEditorWidth - OverscaleGuard;
        ImageClipHeight = ImageEditorHeight - OverscaleGuard;
    }

    public double ImageAreaWidth { get; }
    public double ImageAreaHeight { get; }
    public double ImageEditorWidth { get; }
    public double ImageEditorHeight { get; }
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
