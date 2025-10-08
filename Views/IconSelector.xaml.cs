using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class IconSelector : UserControl
{
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(IconSelector),
            new PropertyMetadata(null, OnSelectedValueChanged));

    public static readonly DependencyProperty IconOptionsProperty =
        DependencyProperty.Register(
            nameof(IconOptions),
            typeof(IEnumerable),
            typeof(IconSelector),
            new PropertyMetadata(null, OnIconOptionsChanged));

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public IEnumerable? IconOptions
    {
        get => (IEnumerable?)GetValue(IconOptionsProperty);
        set => SetValue(IconOptionsProperty, value);
    }

    public IconSelector()
    {
        this.InitializeComponent();
        this.Loaded += IconSelector_Loaded;
    }

    private void IconSelector_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSelectedIcon();
        UpdateGridViewSelection();
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconSelector selector && selector.IsLoaded)
        {
            selector.UpdateSelectedIcon();
            selector.UpdateGridViewSelection();
        }
    }

    private static void OnIconOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconSelector selector)
        {
            selector.IconGridView.ItemsSource = e.NewValue as IEnumerable;
            if (selector.IsLoaded)
            {
                selector.UpdateGridViewSelection();
            }
        }
    }

    private void UpdateSelectedIcon()
    {
        if (SelectedValue == null || IconOptions == null)
            return;

        foreach (var option in IconOptions)
        {
            var enumValueProp = option.GetType().GetProperty("EnumValue");
            var iconPathProp = option.GetType().GetProperty("IconPath");

            if (enumValueProp?.GetValue(option)?.Equals(SelectedValue) == true)
            {
                var iconPath = iconPathProp?.GetValue(option) as string ?? string.Empty;
                if (!string.IsNullOrEmpty(iconPath))
                {
                    var uri = new Uri($"ms-appx:///{iconPath}");
                    SelectedIconImage.Source = new BitmapImage(uri);
                }
                break;
            }
        }
    }

    private void UpdateGridViewSelection()
    {
        if (SelectedValue == null || IconOptions == null)
            return;

        foreach (var option in IconOptions)
        {
            var enumValueProp = option.GetType().GetProperty("EnumValue");
            if (enumValueProp?.GetValue(option)?.Equals(SelectedValue) == true)
            {
                IconGridView.SelectedItem = option;
                break;
            }
        }
    }

    private void SelectorButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateGridViewSelection();
        IconPopup.IsOpen = true;
    }

    private void IconGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IconGridView.SelectedItem != null)
        {
            var selectedOption = IconGridView.SelectedItem;
            var enumValueProp = selectedOption.GetType().GetProperty("EnumValue");
            var iconPathProp = selectedOption.GetType().GetProperty("IconPath");

            if (enumValueProp != null)
            {
                SelectedValue = enumValueProp.GetValue(selectedOption);

                var iconPath = iconPathProp?.GetValue(selectedOption) as string ?? string.Empty;
                if (!string.IsNullOrEmpty(iconPath))
                {
                    var uri = new Uri($"ms-appx:///{iconPath}");
                    SelectedIconImage.Source = new BitmapImage(uri);
                }
            }

            IconPopup.IsOpen = false;
        }
    }
}
