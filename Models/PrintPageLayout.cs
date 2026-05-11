namespace WhiteboardProjectBuilder.Models;

/// <summary>
/// A single printed page modeled as a 2×4 grid of Small-sized cells.
/// Huge = 2×4, Large = 2×2, Medium = 1×2, Small = 1×1 (cols × rows).
/// </summary>
public sealed class PrintPageLayout
{
    public const int Columns = 2;
    public const int Rows = 4;

    public List<PrintSlotPlacement> Placements { get; } = new();
}
