using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Constants;
using WhiteboardProjectBuilder.Enums;

namespace WhiteboardProjectBuilder.ViewModels;

public sealed partial class WhiteboardItemSelectorViewModel : ObservableObject
{
    public WhiteboardItemSelectorViewModel(WhiteboardItemSize size, IEnumerable<WhiteboardItemType> options)
    {
        LayoutSize = size;
        Options = options.Select(SelectorOption.For).ToList();
    }

    public Guid Id { get; } = Guid.NewGuid();

    public WhiteboardItemSize LayoutSize { get; }

    public IReadOnlyList<SelectorOption> Options { get; }

    public double Width => WhiteboardItemSizes.WidthOf(LayoutSize);
    public double Height => WhiteboardItemSizes.HeightOf(LayoutSize);

    public event EventHandler<WhiteboardItemType>? ItemTypeSelected;
    public event EventHandler? CancelRequested;

    [RelayCommand]
    private void Select(WhiteboardItemType type) => ItemTypeSelected?.Invoke(this, type);

    [RelayCommand]
    private void Cancel() => CancelRequested?.Invoke(this, EventArgs.Empty);
}

public sealed record SelectorOption(WhiteboardItemType Type, string Label, string Glyph)
{
    public static SelectorOption For(WhiteboardItemType type) => type switch
    {
        WhiteboardItemType.Project   => new SelectorOption(type, "Project",   ""),
        WhiteboardItemType.TaskItem  => new SelectorOption(type, "Task",      ""),
        WhiteboardItemType.FullImage => new SelectorOption(type, "Image",     ""),
        _ => throw new NotSupportedException($"No selector option for {type}.")
    };
}
