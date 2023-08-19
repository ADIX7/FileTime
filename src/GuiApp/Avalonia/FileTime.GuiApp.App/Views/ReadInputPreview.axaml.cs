using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTime.GuiApp.App.Views;

public partial class ReadInputPreview : UserControl
{
    public ReadInputPreview()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}