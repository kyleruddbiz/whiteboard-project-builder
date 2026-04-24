using CommunityToolkit.Mvvm.ComponentModel;

namespace WhiteboardProjectBuilder.ViewModels;

/// <summary>
/// Print-only grouping that pairs up to two tasks into a single print slot. Built
/// on demand by <see cref="MainPageViewModel.BuildPrintSlots"/>; never persisted.
/// </summary>
public partial class TaskSlotViewModel : ObservableObject, IPrintSlot
{
    [ObservableProperty]
    private TaskItemViewModel topTask = null!;

    [ObservableProperty]
    private TaskItemViewModel? bottomTask;

    public bool HasBottomTask => BottomTask != null;

    partial void OnBottomTaskChanged(TaskItemViewModel? value)
    {
        OnPropertyChanged(nameof(HasBottomTask));
    }
}
