namespace WhiteboardProjectBuilder.ViewModels;

/// <summary>
/// Marker interface for types that can occupy one slot on a printed page.
/// The print pipeline lays out up to four slots per page in a 2x2 grid; each slot
/// is either a single <see cref="ProjectItemViewModel"/> or a
/// <see cref="TaskSlotViewModel"/> that stacks two tasks into one cell.
/// </summary>
public interface IPrintSlot
{
}
