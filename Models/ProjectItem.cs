using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Models;

public class ProjectItem : WhiteboardItem
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public required ProjectSize Size { get; set; }
    public required ProjectValue Value { get; set; }
    public DateTime? DueDate { get; set; }
}
