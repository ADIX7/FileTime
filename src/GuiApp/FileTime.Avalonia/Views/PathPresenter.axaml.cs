using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FileTime.Avalonia.Views
{
    public partial class PathPresenter : UserControl
    {
        public PathPresenter()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
