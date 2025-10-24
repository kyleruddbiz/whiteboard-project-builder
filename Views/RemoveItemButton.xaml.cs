using System.Windows.Input;
using Microsoft.UI.Input;
using WhiteboardProjectBuilder.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace WhiteboardProjectBuilder.Views;

/// <summary>
/// Shared remove button for whiteboard items.
/// Supports normal click (archive) and Ctrl+Click (force delete).
/// </summary>
public sealed partial class RemoveItemButton : UserControl
{
    public static readonly DependencyProperty WhiteboardItemProperty =
        DependencyProperty.Register(
            nameof(WhiteboardItem),
            typeof(WhiteboardItemViewModelBase),
            typeof(RemoveItemButton),
            new PropertyMetadata(null));

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

    private static void OnIsCtrlKeyPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RemoveItemButton button)
        {
            button.UpdateVisualState();
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

    private void UpdateVisualState()
    {
        if (WhiteboardItem?.IsArchived == true)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            return;
        }

        string stateName = IsCtrlKeyPressed ? "CtrlPressed" : "Normal";
        VisualStateManager.GoToState(this, stateName, true);
    }
}
