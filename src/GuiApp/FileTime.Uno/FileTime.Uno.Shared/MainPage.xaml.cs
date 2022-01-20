using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using FileTime.Uno.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using FileTime.Uno.Misc;
using System.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileTime.Uno
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IServiceProvider _serviceProvider;
        private InputElementWrapper _inputElementWrapper;
        private MainPageViewModel ViewModel => DataContext as MainPageViewModel;

        private ResourceDictionary _lightTheme;
        private ResourceDictionary _solarizedDark;
        private ResourceDictionary _currentTheme;
        public MainPage()
        {
            _serviceProvider = App.ServiceProvider;

            this.InitializeComponent();
            DataContext = _serviceProvider.GetService<MainPageViewModel>();

            ViewModel.FocusDefaultElement = () => (FindName(nameof(CurrentItems)) as FrameworkElement)?.Focus(FocusState.Programmatic);

            foreach (var asd in Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries)
            {
                if (asd is ResourceDictionary resourceDictionary && resourceDictionary.Source != null)
                {
                    if (resourceDictionary.Source.LocalPath == "/Files/Themes/DefaultLight.xaml")
                    {
                        _lightTheme = resourceDictionary;
                    }
                    else if (resourceDictionary.Source.LocalPath == "/Files/Themes/SolarizedDark.xaml")
                    {
                        _solarizedDark = resourceDictionary;
                    }
                }
            }

            _currentTheme = _lightTheme;
            Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Remove(_solarizedDark);
            //Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Remove(_lightTheme);
        }

        private void RootContainer_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentItems.Focus(FocusState.Programmatic);
        }

        private void CurrentItems_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = ViewModel.ProcessKeyDown(e.Key) || e.Handled;
        }

        private async void CurrentItems_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = await ViewModel.ProcessKeyUp(e.Key) || e.Handled;
        }

        private void InputText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter && _inputElementWrapper == ViewModel.Inputs.Last())
            {
                ViewModel.ProcessInputs();
            }
        }

        private void InpuText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox inputText && inputText.DataContext is InputElementWrapper inputElementWrapper)
            {
                _inputElementWrapper = inputElementWrapper;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(_currentTheme == _lightTheme)
            {
                Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(_solarizedDark);
                Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Remove(_lightTheme);

                _currentTheme = _solarizedDark;
            }
            else
            {
                Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(_lightTheme);
                Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Remove(_solarizedDark);

                _currentTheme = _lightTheme;
            }

            this.RequestedTheme = ElementTheme.Dark;
            this.RequestedTheme = ElementTheme.Light;
        }
    }
}
