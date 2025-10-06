namespace WhiteboardProjectBuilder.Views;

public sealed partial class SaveImageDialog : ContentDialog
{
    public string FileName { get; private set; } = string.Empty;

    public SaveImageDialog(string defaultFileName)
    {
        this.InitializeComponent();

        FileNameTextBox.Text = defaultFileName;
        FileName = defaultFileName;

        UpdatePreview();
    }

    private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FileName = FileNameTextBox.Text;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var fileName = string.IsNullOrWhiteSpace(FileName) ? "[empty]" : FileName;

        if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".png";
        }

        PreviewTextBlock.Text = $"Will be saved as: {fileName}";
    }
}
