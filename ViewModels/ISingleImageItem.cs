namespace WhiteboardProjectBuilder.ViewModels;

public interface ISingleImageItem
{
    string Image { get; set; }
    double ImageZoomFactor { get; set; }
    double ImageOffsetX { get; set; }
    double ImageOffsetY { get; set; }
    double ImageClipWidth { get; }
    double ImageClipHeight { get; }
}
