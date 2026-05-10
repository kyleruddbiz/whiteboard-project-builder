using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.Constants;

/// <summary>
/// Declares which <see cref="WhiteboardItemSize"/> values each
/// <see cref="WhiteboardItemType"/> supports.
/// </summary>
public static class ItemTypeSizeRegistry
{
    public static readonly IReadOnlyDictionary<WhiteboardItemType, IReadOnlyList<WhiteboardItemSize>> SupportedSizes =
        new Dictionary<WhiteboardItemType, IReadOnlyList<WhiteboardItemSize>>
        {
            [WhiteboardItemType.Project]  = [WhiteboardItemSize.Medium],
            [WhiteboardItemType.TaskItem] = [WhiteboardItemSize.Small],
        };

    public static IEnumerable<WhiteboardItemType> ItemTypesForSize(WhiteboardItemSize size) =>
        SupportedSizes.Where(kv => kv.Value.Contains(size)).Select(kv => kv.Key);

    public static IEnumerable<WhiteboardItemSize> ActiveSizes() =>
        SupportedSizes.Values.SelectMany(sizes => sizes).Distinct();
}
