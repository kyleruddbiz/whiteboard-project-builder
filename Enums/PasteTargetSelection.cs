using Microsoft.UI.Xaml.Controls;

namespace WhiteboardProjectBuilder.Enums;

/// <summary>
/// Represents the user's selection for which SomedayMaybe item should receive a pasted image.
/// </summary>
public enum PasteTargetSelection
{
    Top,
    Bottom,
    Cancel
}

public static class PasteTargetSelectionExtensions
{
    /// <summary>
    /// Converts a ContentDialogResult to a PasteTargetSelection.
    /// </summary>
    /// <param name="result">The dialog result to convert.</param>
    /// <returns>The corresponding PasteTargetSelection value.</returns>
    public static PasteTargetSelection ToPasteTargetSelection(this ContentDialogResult result)
    {
        return result switch
        {
            ContentDialogResult.Primary => PasteTargetSelection.Top,
            ContentDialogResult.Secondary => PasteTargetSelection.Bottom,
            _ => PasteTargetSelection.Cancel
        };
    }
}
