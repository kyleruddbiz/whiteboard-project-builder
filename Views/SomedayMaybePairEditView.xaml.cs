using Microsoft.UI.Xaml.Input;
using WhiteboardProjectBuilder.ViewModels;

namespace WhiteboardProjectBuilder.Views;

public sealed partial class SomedayMaybePairEditView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(SomedayMaybePairViewModel),
        typeof(SomedayMaybePairEditView),
        new PropertyMetadata(null));

    public SomedayMaybePairViewModel? ViewModel
    {
        get => (SomedayMaybePairViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler<int>? ImageReplaceRequested;

    public SomedayMaybePairEditView()
    {
        this.InitializeComponent();
        this.DataContext = this;
    }

    private void TopItemEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, 0); // 0 = Top item
    }

    private void BottomItemEditor_ImageReplaceRequested(object sender, EventArgs e)
    {
        ImageReplaceRequested?.Invoke(this, 1); // 1 = Bottom item
    }

    private void EscapeAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel != null)
        {
            ViewModel.IsEditing = false;
        }
        args.Handled = true;
    }
}
