namespace WhiteboardProjectBuilder.Views;

public sealed partial class FloatingLabelTextBox : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(FloatingLabelTextBox),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(FloatingLabelTextBox),
        new PropertyMetadata(string.Empty, OnTextChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public FloatingLabelTextBox()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public new bool Focus(FocusState value)
    {
        return InnerTextBox.Focus(value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((FloatingLabelTextBox)d).UpdateFloatState();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateFloatState(useTransitions: false);
    }

    private void InnerTextBox_FocusChanged(object sender, RoutedEventArgs e)
    {
        UpdateFloatState();
    }

    private void UpdateFloatState(bool useTransitions = true)
    {
        bool shouldFloat = InnerTextBox.FocusState != FocusState.Unfocused || !string.IsNullOrEmpty(Text);
        VisualStateManager.GoToState(this, shouldFloat ? "Floating" : "Resting", useTransitions);
    }
}
