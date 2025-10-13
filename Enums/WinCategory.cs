using Microsoft.UI;
using Windows.UI;

namespace WhiteboardProjectBuilder.Enums;

public enum WinCategory
{
    PoorWin,
    Win,
    BigWin
}

public static class WinCategoryExtensions
{
    public static WinCategory GetWinCategory(ProjectSize size, ProjectValue value)
    {
        return (value, size) switch
        {
            (ProjectValue.Good, ProjectSize.Small) => WinCategory.Win,
            (ProjectValue.Good, ProjectSize.Medium) => WinCategory.Win,
            (ProjectValue.Good, ProjectSize.Large) => WinCategory.PoorWin,
            (ProjectValue.Good, ProjectSize.Huge) => WinCategory.PoorWin,

            (ProjectValue.Great, ProjectSize.Small) => WinCategory.BigWin,
            (ProjectValue.Great, ProjectSize.Medium) => WinCategory.Win,
            (ProjectValue.Great, ProjectSize.Large) => WinCategory.Win,
            (ProjectValue.Great, ProjectSize.Huge) => WinCategory.PoorWin,

            (ProjectValue.Grand, ProjectSize.Small) => WinCategory.BigWin,
            (ProjectValue.Grand, ProjectSize.Medium) => WinCategory.BigWin,
            (ProjectValue.Grand, ProjectSize.Large) => WinCategory.BigWin,
            (ProjectValue.Grand, ProjectSize.Huge) => WinCategory.Win,

            _ => throw new ArgumentOutOfRangeException(nameof(size), $"Unexpected combination: {value}, {size}")
        };
    }

    public static Color GetBorderColor(this WinCategory category)
    {
        return category switch
        {
            WinCategory.PoorWin => ColorHelper.FromArgb(255, 0xEF, 0x71, 0x00),
            WinCategory.Win => Colors.Transparent,
            WinCategory.BigWin => ColorHelper.FromArgb(255, 0xFF, 0xC0, 0x00),
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }
}
