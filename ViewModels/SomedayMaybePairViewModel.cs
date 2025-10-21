using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhiteboardProjectBuilder.Enums;
using WhiteboardProjectBuilder.Models;

namespace WhiteboardProjectBuilder.ViewModels;

public partial class SomedayMaybePairViewModel : WhiteboardItemViewModelBase
{
    [ObservableProperty]
    private SomedayMaybeViewModel topItem = null!;

    [ObservableProperty]
    private SomedayMaybeViewModel? bottomItem;

    public bool HasBottomItem => BottomItem != null;
    public bool ShowAddBottomButton => !HasBottomItem && IsEditing;

    partial void OnTopItemChanged(SomedayMaybeViewModel? oldValue, SomedayMaybeViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnItemDataChanged;
        }

        if (newValue != null)
        {
            newValue.DataChanged += OnItemDataChanged;
        }

        OnPropertyChanged(nameof(HasBottomItem));
        OnPropertyChanged(nameof(ShowAddBottomButton));
        RaiseDataChanged();
    }

    partial void OnBottomItemChanged(SomedayMaybeViewModel? oldValue, SomedayMaybeViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.DataChanged -= OnItemDataChanged;
        }

        if (newValue != null)
        {
            newValue.DataChanged += OnItemDataChanged;
        }

        OnPropertyChanged(nameof(HasBottomItem));
        OnPropertyChanged(nameof(ShowAddBottomButton));
        RaiseDataChanged();
    }

    private void OnItemDataChanged(object? sender, EventArgs e)
    {
        RaiseDataChanged();
    }

    [RelayCommand]
    private void AddBottomItem()
    {
        if (BottomItem == null)
        {
            BottomItem = new SomedayMaybeViewModel
            {
                CreatedDate = DateTime.Today
            };
        }
    }

    [RelayCommand]
    private void RemoveBottomItem()
    {
        BottomItem = null;
    }

    /// <summary>
    /// Gets the type of this whiteboard item.
    /// </summary>
    public override WhiteboardItemType GetItemType() => WhiteboardItemType.SomedayMaybe;

    /// <summary>
    /// Converts this ViewModel to a Model for serialization.
    /// </summary>
    public override IWhiteboardItem ToModel()
    {
        return new SomedayMaybePair
        {
            TopItem = TopItem.ToModel(),
            BottomItem = BottomItem?.ToModel(),
            CreatedDate = CreatedDate,
            IsArchived = IsArchived
        };
    }

    /// <summary>
    /// Creates a ViewModel from a Model.
    /// </summary>
    public static SomedayMaybePairViewModel FromModel(IWhiteboardItem item)
    {
        if (item is not SomedayMaybePair model)
        {
            throw new ArgumentException($"Expected SomedayMaybePair but got {item.GetType().Name}", nameof(item));
        }

        var viewModel = new SomedayMaybePairViewModel
        {
            TopItem = SomedayMaybeViewModel.FromModel(model.TopItem),
            BottomItem = model.BottomItem != null ? SomedayMaybeViewModel.FromModel(model.BottomItem) : null,
            CreatedDate = model.CreatedDate,
            IsArchived = model.IsArchived
        };

        return viewModel;
    }
}
