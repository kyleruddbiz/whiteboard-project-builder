namespace WhiteboardProjectBuilder.Enums;

public enum ProjectSize
{
    Small,
    Medium,
    Large,
    Huge
}

public static class ProjectSizeExtensions
{
    public static string ToIcon(this ProjectSize size)
    {
        return size switch
        {
            ProjectSize.Small => "Assets/Sizes/Small Project.jpg",
            ProjectSize.Medium => "Assets/Sizes/Medium Project.jpg",
            ProjectSize.Large => "Assets/Sizes/Large Project.jpg",
            ProjectSize.Huge => "Assets/Sizes/Huge Project.jpg",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}
