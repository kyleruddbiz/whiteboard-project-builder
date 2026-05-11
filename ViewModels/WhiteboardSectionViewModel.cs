using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

/// <summary>
/// One home-page section: all items at a single <see cref="WhiteboardItemSize"/>.
/// </summary>
public sealed partial class WhiteboardSectionViewModel(
    WhiteboardItemSize size,
    WhiteboardItemWorkspaceViewModel workspace) : ObservableObject
{
    public WhiteboardItemSize Size { get; } = size;

    public ObservableCollection<GridItemWrapper> Items { get; } = [];

    [ObservableProperty]
    private GridItemWrapper? selectedItem;

    partial void OnSelectedItemChanged(GridItemWrapper? value)
    {
        workspace.SelectedItem = value?.WhiteboardItem;
    }
}
