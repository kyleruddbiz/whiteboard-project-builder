namespace WhiteboardProjectBuilder.Enums;

public enum ImageEditMode
{
    Pan,
    Zoom
}

public static class ImageEditModeExtensions
{
    public static string ToIcon(this ImageEditMode mode)
    {
        return mode switch
        {
            ImageEditMode.Pan => "\uE7C2",
            ImageEditMode.Zoom => "\uE71E",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
