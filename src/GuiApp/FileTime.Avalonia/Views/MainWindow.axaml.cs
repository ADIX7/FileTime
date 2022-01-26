using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FileTime.Avalonia.ViewModels;

namespace FileTime.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainPageViewModel? ViewModel
        {
            get => DataContext as MainPageViewModel;
            set
            {
                if (value != DataContext)
                {
                    if (DataContext is MainPageViewModel currentViewModel)
                    {
                        currentViewModel.FocusDefaultElement = null;
                    }

                    DataContext = value;

                    if (value != null)
                    {
                        //value.FocusDefaultElement = () => this.FindControl<ListBox>("CurrentItems")?.Focus();
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void OnKeyDown(object sender, KeyEventArgs e)
        {
            await ViewModel?.ProcessKeyDown(e.Key, e.KeyModifiers);
        }

        public async void OnKeyUp(object sender, KeyEventArgs e)
        {
            await ViewModel?.ProcessKeyUp(e.Key, e.KeyModifiers);
        }
    }
}