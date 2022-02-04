using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace FileTime.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow>? _logger;
        public MainPageViewModel? ViewModel
        {
            get => DataContext as MainPageViewModel;
            set
            {
                if (value != DataContext)
                {
                    DataContext = value;
                }
            }
        }

        private InputElementWrapper? _inputElementWrapper;

        public MainWindow()
        {
            _logger = App.ServiceProvider.GetService<ILogger<MainWindow>>();
            _logger?.LogInformation($"Starting {nameof(MainWindow)} initialization...");
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_inputElementWrapper == null)
            {
                ViewModel!.ProcessKeyDown(e.Key, e.KeyModifiers, h => e.Handled = h);
            }
        }

        private void InputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _inputElementWrapper == ViewModel!.Inputs.Last())
            {
                ViewModel.ProcessInputs();
                _inputElementWrapper = null;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && _inputElementWrapper == ViewModel!.Inputs.Last())
            {
                ViewModel.CancelInputs();
                _inputElementWrapper = null;
                e.Handled = true;
            }
        }

        private void InputText_GotFocus(object sender, GotFocusEventArgs e)
        {
            if (sender is TextBox inputText && inputText.DataContext is InputElementWrapper inputElementWrapper)
            {
                _inputElementWrapper = inputElementWrapper;
            }
        }

        private void InputText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox inputText && inputText.DataContext is InputElementWrapper)
            {
                _inputElementWrapper = null;
            }
        }

        private void InputText_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is TextBox inputText && inputText.DataContext is InputElementWrapper inputElementWrapper && inputElementWrapper == ViewModel!.Inputs.First())
            {
                inputText.Focus();
            }
        }

        private void OnPlacePointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!e.Handled
                && ViewModel != null
                && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
                && sender is StyledElement control
                && control.DataContext is PlaceInfo placeInfo)
            {
                ViewModel.OpenContainer(placeInfo.Container);
                e.Handled = true;
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            try
            {
                ViewModel?.StatePersistence.SaveStates();
            }
            catch { }
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            if (ViewModel is not MainPageViewModel)
            {
                _logger?.LogInformation($"{nameof(MainWindow)} opened, starting {nameof(MainPageViewModel)} initialization...");
                ViewModel = App.ServiceProvider.GetService<MainPageViewModel>();
            }
        }

        private void HeaderPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                BeginMoveDrag(e);
            }
        }
    }
}