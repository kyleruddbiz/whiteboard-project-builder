namespace WhiteboardProjectBuilder.Helpers;

public static class DateTimeHelper
{
    public static string ToMonthDayFormat(DateTime? dateTime)
    {
        return dateTime?.ToString("M/d")
            ?? string.Empty;
    }
}