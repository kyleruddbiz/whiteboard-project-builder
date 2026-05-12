using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Services;

/// <summary>
/// Packs <see cref="IPrintSlot"/>s onto <see cref="PrintPageLayout"/> pages using a
/// first-fit-decreasing strategy ("rocks then sand"): largest footprints placed first,
/// then each slot dropped into the earliest page with room.
/// </summary>
public class PrintPagePackerService
{
    public IReadOnlyList<PrintPageLayout> BuildPrintPages(IEnumerable<IPrintSlot> slots)
    {
        var ordered = slots.OrderByDescending(s => s.LayoutSize);
        var pages = new List<PackingPage>();

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
                var newPage = new PackingPage();
                newPage.TryPlace(slot);
                pages.Add(newPage);
            }
        }

        return pages.Select(p => p.Layout).ToList();
    }

    private static (int rowSpan, int colSpan) FootprintOf(WhiteboardItemSize size) => size switch
    {
        WhiteboardItemSize.Huge => (4, 2),
        WhiteboardItemSize.Large => (2, 2),
        WhiteboardItemSize.Medium => (2, 1),
        WhiteboardItemSize.Small => (1, 1),
        _ => throw new NotSupportedException($"No print footprint defined for size {size}.")
    };

    /// <summary>
    /// Pairs a <see cref="PrintPageLayout"/> with the transient occupancy grid used to decide
    /// placements during a single packing pass. The grid is discarded once packing completes;
    /// only the layout escapes.
    /// </summary>
    private sealed class PackingPage
    {
        private readonly bool[,] occupied = new bool[PrintPageLayout.Rows, PrintPageLayout.Columns];

        public PrintPageLayout Layout { get; } = new();

        public bool TryPlace(IPrintSlot slot)
        {
            var (rowSpan, colSpan) = FootprintOf(slot.LayoutSize);

            for (int r = 0; r <= PrintPageLayout.Rows - rowSpan; r++)
            {
                for (int c = 0; c <= PrintPageLayout.Columns - colSpan; c++)
                {
                    if (IsAreaFree(r, c, rowSpan, colSpan))
                    {
                        MarkOccupied(r, c, rowSpan, colSpan);
                        Layout.Placements.Add(new PrintSlotPlacement(slot, r, c, rowSpan, colSpan));
                        return true;
                    }
                }
            }

            return false;
        }

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
}
