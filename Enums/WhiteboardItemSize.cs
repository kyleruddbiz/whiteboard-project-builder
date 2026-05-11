namespace WhiteboardProjectBuilder.Enums;

public enum WhiteboardItemSize
{
    Small,
    Medium,
    Large,
    Huge
}

public static class WhiteboardItemSizeExtensions
{
    private static readonly IReadOnlyDictionary<WhiteboardItemType, IReadOnlyList<WhiteboardItemSize>> SupportedSizesByType =
        new Dictionary<WhiteboardItemType, IReadOnlyList<WhiteboardItemSize>>
        {
            [WhiteboardItemType.Project]  = [WhiteboardItemSize.Medium],
            [WhiteboardItemType.TaskItem] = [WhiteboardItemSize.Small],
        };

    public static IEnumerable<WhiteboardItemType> SupportedItemTypes(this WhiteboardItemSize size) =>
        SupportedSizesByType.Where(kv => kv.Value.Contains(size)).Select(kv => kv.Key);
}
