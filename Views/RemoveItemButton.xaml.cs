using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Media;
using WhiteboardProjectBuilder.ViewModels;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace WhiteboardProjectBuilder.Views;

/// <summary>
/// Shared remove button for whiteboard items.
/// Supports normal click (archive) and Ctrl+Click (force delete).
/// </summary>
public sealed partial class RemoveItemButton : UserControl, INotifyPropertyChanged
{
    private static readonly SolidColorBrush BlackBrush = new(Colors.Black);
    private static readonly SolidColorBrush CrimsonBrush = new(Color.FromArgb(255, 220, 20, 60));

    public event PropertyChangedEventHandler? PropertyChanged;

    public enum DeleteIconState
    {
        Normal,    // Black X - active item, no CTRL
        Forced,    // Red trash can - active item, CTRL pressed
        Archived   // Red trash can - archived item
    }

    private DeleteIconState _currentDeleteIconState = DeleteIconState.Normal;
    public DeleteIconState CurrentDeleteIconState
    {
        get => _currentDeleteIconState;
        private set
        {
            if (_currentDeleteIconState != value)
            {
                _currentDeleteIconState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconGlyph));
                OnPropertyChanged(nameof(IconForeground));
            }
        }
    }

    public string IconGlyph => CurrentDeleteIconState == DeleteIconState.Normal
        ? "\uE106"  // X
        : "\uE74D"; // Trash can

    public SolidColorBrush IconForeground => CurrentDeleteIconState == DeleteIconState.Normal
        ? BlackBrush
        : CrimsonBrush;
    public static readonly DependencyProperty WhiteboardItemProperty =
        DependencyProperty.Register(
            nameof(WhiteboardItem),
            typeof(WhiteboardItemViewModelBase),
            typeof(RemoveItemButton),
            new PropertyMetadata(null, OnWhiteboardItemChanged));

    public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(
            nameof(RemoveCommand),
            typeof(ICommand),
            typeof(RemoveItemButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ForceRemoveCommandProperty =
        DependencyProperty.Register(
            nameof(ForceRemoveCommand),
            typeof(ICommand),
            typeof(RemoveItemButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsCtrlKeyPressedProperty =
        DependencyProperty.Register(
            nameof(IsCtrlKeyPressed),
            typeof(bool),
            typeof(RemoveItemButton),
            new PropertyMetadata(false, OnIsCtrlKeyPressedChanged));

    public WhiteboardItemViewModelBase? WhiteboardItem
    {
        get => (WhiteboardItemViewModelBase?)GetValue(WhiteboardItemProperty);
        set => SetValue(WhiteboardItemProperty, value);
    }

    public ICommand? RemoveCommand
    {
        get => (ICommand?)GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public ICommand? ForceRemoveCommand
    {
        get => (ICommand?)GetValue(ForceRemoveCommandProperty);
        set => SetValue(ForceRemoveCommandProperty, value);
    }

    public bool IsCtrlKeyPressed
    {
        get => (bool)GetValue(IsCtrlKeyPressedProperty);
        set => SetValue(IsCtrlKeyPressedProperty, value);
    }

    public RemoveItemButton()
    {
        InitializeComponent();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static void OnWhiteboardItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RemoveItemButton button)
        {
            button.UpdateIconState();
        }
    }

    private static void OnIsCtrlKeyPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RemoveItemButton button)
        {
            button.UpdateIconState();
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (WhiteboardItem == null)
        {
            return;
        }

        bool isCtrlPressed = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Control)
            .HasFlag(CoreVirtualKeyStates.Down);

        ICommand? command;
        if (isCtrlPressed && !WhiteboardItem.IsArchived)
        {
            command = ForceRemoveCommand;
        }
        else
        {
            command = RemoveCommand;
        }

        if (command?.CanExecute(WhiteboardItem) == true)
        {
            command.Execute(WhiteboardItem);
        }
    }

    private void UpdateIconState()
    {
        if (WhiteboardItem?.IsArchived == true)
        {
            CurrentDeleteIconState = DeleteIconState.Archived;
        }
        else if (IsCtrlKeyPressed)
        {
            CurrentDeleteIconState = DeleteIconState.Forced;
        }
        else
        {
            CurrentDeleteIconState = DeleteIconState.Normal;
        }
    }
}
