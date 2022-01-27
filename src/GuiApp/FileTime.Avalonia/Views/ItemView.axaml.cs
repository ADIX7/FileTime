using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTime.Avalonia.Views
{
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
