namespace WhiteboardProjectBuilder.Enums;

public enum ProjectValue
{
    Good,
    Great,
    Grand
}

public static class ProjectValueExtensions
{
    public static string ToIcon(this ProjectValue value)
    {
        return value switch
        {
            ProjectValue.Good => "Assets/Values/Good Project.jpg",
            ProjectValue.Great => "Assets/Values/Great Project.jpg",
            ProjectValue.Grand => "Assets/Values/Grand Project.jpg",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
