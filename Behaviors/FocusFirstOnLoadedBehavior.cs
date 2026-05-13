using Microsoft.UI.Xaml.Input;

namespace WhiteboardProjectBuilder.Behaviors;

public static class FocusFirstOnLoadedBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FocusFirstOnLoadedBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value)
        => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element)
        => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.Loaded -= OnLoaded;

        if (e.NewValue is true)
        {
            element.Loaded += OnLoaded;
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject root)
        {
            return;
        }

        if (FocusManager.FindFirstFocusableElement(root) is Control firstFocusable)
        {
            firstFocusable.Focus(FocusState.Programmatic);
        }
    }
}
