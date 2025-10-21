namespace WhiteboardProjectBuilder.Models;

public class SomedayMaybePair : IWhiteboardItem
{
    public required SomedayMaybe TopItem { get; set; }
    public SomedayMaybe? BottomItem { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Today;
    public bool IsArchived { get; set; }
}
