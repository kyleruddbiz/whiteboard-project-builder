namespace WhiteboardProjectBuilder.Models;

public class TaskItem
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public required string Image { get; set; }
    public ImageTransform? Transform { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Today;
}
