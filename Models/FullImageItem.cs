using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Models;

public class FullImageItem : IWhiteboardItem
{
    public required string Image { get; set; }
    public ImageTransform? Transform { get; set; }
    public required WhiteboardItemSize Size { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Today;
    public bool IsArchived { get; set; }
}
