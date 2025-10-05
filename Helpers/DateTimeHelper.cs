namespace WhiteboardProjectBuilder.Helpers;

public static class DateTimeHelper
{
    public static string ToDateFormat(DateTime? dateTime)
    {
        return dateTime?.ToShortDateString()
            ?? string.Empty;
    }
}