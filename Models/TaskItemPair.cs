namespace WhiteboardProjectBuilder.Models;

public class TaskItemPair : IWhiteboardItem
{
    public required TaskItem TopItem { get; set; }
    public TaskItem? BottomItem { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Today;
    public bool IsArchived { get; set; }
}
