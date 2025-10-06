namespace WhiteboardProjectBuilder.Models.Serialization;

/// <summary>
/// Root container for all whiteboard data serialization.
/// </summary>
public class WhiteboardData
{
    public int Version { get; set; } = 1;
    public List<ProjectItem> Projects { get; set; } = [];
}
