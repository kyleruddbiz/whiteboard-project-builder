namespace WhiteboardProjectBuilder.Extensions;

public static class DateTimeExtensions
{
    public static string ToDateFormat(this DateTime? dateTime)
    {
        return dateTime?.ToShortDateString()
            ?? string.Empty;
    }
}
