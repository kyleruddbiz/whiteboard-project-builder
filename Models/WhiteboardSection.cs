using System.Collections.ObjectModel;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Models;

/// <summary>
/// One home-page section: all items at a single <see cref="WhiteboardItemSize"/>.
/// </summary>
public sealed class WhiteboardSection(WhiteboardItemSize size)
{
    public WhiteboardItemSize Size { get; } = size;

    public ObservableCollection<GridItemWrapper> Items { get; } = [];
}
