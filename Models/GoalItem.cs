namespace WhiteboardProjectBuilder.Models;

public class GoalItem : WhiteboardItem
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
}
