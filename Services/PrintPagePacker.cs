using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Placement of a single <see cref="IPrintSlot"/> on a printed page, in Small-unit grid coordinates
/// (2 columns × 4 rows per page; each cell is one Small item's footprint).
/// </summary>
public sealed record PrintSlotPlacement(IPrintSlot Slot, int Row, int Col, int RowSpan, int ColSpan);

/// <summary>
/// A single printed page modeled as a 2×4 grid of Small-sized cells.
/// Huge = 2×4, Large = 2×2, Medium = 1×2, Small = 1×1 (cols × rows).
/// </summary>
public sealed class PrintPageLayout
{
    public const int Columns = 2;
    public const int Rows = 4;

    private readonly bool[,] occupied = new bool[Rows, Columns];

    public List<PrintSlotPlacement> Placements { get; } = new();

    public bool TryPlace(IPrintSlot slot)
    {
        var (rowSpan, colSpan) = FootprintOf(slot.LayoutSize);

        for (int r = 0; r <= Rows - rowSpan; r++)
        {
            for (int c = 0; c <= Columns - colSpan; c++)
            {
                if (IsAreaFree(r, c, rowSpan, colSpan))
                {
                    MarkOccupied(r, c, rowSpan, colSpan);
                    Placements.Add(new PrintSlotPlacement(slot, r, c, rowSpan, colSpan));
                    return true;
                }
            }
        }

        return false;
    }

    private static (int rowSpan, int colSpan) FootprintOf(WhiteboardItemSize size) => size switch
    {
        WhiteboardItemSize.Huge => (4, 2),
        WhiteboardItemSize.Large => (2, 2),
        WhiteboardItemSize.Medium => (2, 1),
        WhiteboardItemSize.Small => (1, 1),
        _ => throw new NotSupportedException($"No print footprint defined for size {size}.")
    };

    private bool IsAreaFree(int row, int col, int rowSpan, int colSpan)
    {
        for (int r = row; r < row + rowSpan; r++)
        {
            for (int c = col; c < col + colSpan; c++)
            {
                if (occupied[r, c])
                    return false;
            }
        }
        return true;
    }

    private void MarkOccupied(int row, int col, int rowSpan, int colSpan)
    {
        for (int r = row; r < row + rowSpan; r++)
        {
            for (int c = col; c < col + colSpan; c++)
            {
                occupied[r, c] = true;
            }
        }
    }
}

public static class PrintPagePacker
{
    /// <summary>
    /// First-fit-decreasing packer: orders slots largest-first ("rocks then sand"),
    /// then places each into the earliest page that has room.
    /// </summary>
    public static IReadOnlyList<PrintPageLayout> BuildPrintPages(IEnumerable<IPrintSlot> slots)
    {
        var ordered = slots.OrderByDescending(s => s.LayoutSize).ToList();
        var pages = new List<PrintPageLayout>();

        foreach (var slot in ordered)
        {
            bool placed = false;
            foreach (var page in pages)
            {
                if (page.TryPlace(slot))
                {
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                var newPage = new PrintPageLayout();
                newPage.TryPlace(slot);
                pages.Add(newPage);
            }
        }

        return pages;
    }
}
