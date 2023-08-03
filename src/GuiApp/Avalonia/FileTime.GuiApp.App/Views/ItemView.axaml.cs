using Avalonia;
using Avalonia.Controls;

namespace FileTime.GuiApp.App.Views;

public partial class ItemView : UserControl
{
    public static readonly StyledProperty<bool> ShowAttributesProperty = AvaloniaProperty.Register<ItemView, bool>(nameof(ShowAttributes), true);

    public bool ShowAttributes
    {
        get => GetValue(ShowAttributesProperty);
        set => SetValue(ShowAttributesProperty, value);
    }

    public ItemView()
    {
        InitializeComponent();
    }
}