using System.Windows.Input;
using Microsoft.UI.Xaml.Input;

namespace WhiteboardProjectBuilder.Behaviors;

public static class KeyboardAcceleratorCommandBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(KeyboardAcceleratorCommandBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject dependencyObject, ICommand value)
        => dependencyObject.SetValue(CommandProperty, value);

    public static ICommand GetCommand(DependencyObject dependencyObject)
        => (ICommand)dependencyObject.GetValue(CommandProperty);

    private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is KeyboardAccelerator accelerator)
        {
            accelerator.Invoked -= OnInvoked;

            if (e.NewValue is ICommand)
            {
                accelerator.Invoked += OnInvoked;
            }
        }
    }

    private static void OnInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var command = GetCommand(sender);
        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
            args.Handled = true;
        }
    }
}
