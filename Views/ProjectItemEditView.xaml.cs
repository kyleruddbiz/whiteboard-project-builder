using Microsoft.UI.Xaml.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using WhiteboardProjectBuilder.Services;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class ProjectItemEditView : UserControl
{
    private readonly ImageStorageService imageStorageService;

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProjectItemViewModel),
            typeof(ProjectItemEditView),
            new PropertyMetadata(null));

    public ProjectItemViewModel? ViewModel
    {
        get => (ProjectItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ProjectItemEditView()
    {
        this.InitializeComponent();
        imageStorageService = new ImageStorageService();

        DataContextChanged += (s, e) =>
        {
            if (e.NewValue is ProjectItemViewModel vm)
            {
                ViewModel = vm;
            }
        };

        this.Loaded += ProjectItemEditView_Loaded;
    }

    private void ProjectItemEditView_Loaded(object sender, RoutedEventArgs e)
    {
        // Set focus when the control is fully loaded
        TitleTextBox.Focus(FocusState.Programmatic);
    }

    private async void ChooseImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        var window = App.MainWindow;
        if (window == null) return;

        var openPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail
        };

        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");
        openPicker.FileTypeFilter.Add(".bmp");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            try
            {
                string imageUri = await imageStorageService.SaveImageAsync(file.Path, file.Name);
                ViewModel.Image = imageUri;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save image: {ex.Message}");
            }
        }
    }

    private void EscapeAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel?.IsEditing == true)
        {
            ViewModel.IsEditing = false;
            args.Handled = true;
        }
    }
}