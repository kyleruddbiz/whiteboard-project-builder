using Microsoft.UI.Xaml.Controls;

namespace WhiteboardProjectBuilder.Views;

/// <summary>
/// Dialog that allows the user to select which SomedayMaybe item (Top or Bottom) should receive a pasted image.
/// </summary>
public sealed partial class PasteSelectionDialog : ContentDialog
{
    public PasteSelectionDialog()
    {
        InitializeComponent();
    }
}
