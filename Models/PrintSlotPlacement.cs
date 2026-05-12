using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Models;

/// <summary>
/// Placement of a single <see cref="IPrintSlot"/> on a printed page, in Small-unit grid coordinates
/// (2 columns × 4 rows per page; each cell is one Small item's footprint).
/// </summary>
public sealed record PrintSlotPlacement(IPrintSlot Slot, int Row, int Col, int RowSpan, int ColSpan);
