using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FileTime.App.Core.Models;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            if (e.Key == Key.Enter && _inputElementWrapper == ViewModel!.AppState.Inputs.Last())
            {
                ViewModel.ProcessInputs();
                _inputElementWrapper = null;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && _inputElementWrapper == ViewModel!.AppState.Inputs.Last())
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
            if (sender is TextBox inputText && inputText.IsVisible && inputText.DataContext is InputElementWrapper inputElementWrapper && inputElementWrapper == ViewModel!.AppState.Inputs[0])
            {
                inputText.Focus();
            }
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        private void OnHasContainerPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!e.Handled
                && ViewModel != null
                && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
                && sender is StyledElement control)
            {
                if (control.DataContext is IHaveContainer hasContainer
                && hasContainer.Container is not null)
                {
                    ViewModel.CommandHandlerService.OpenContainer(hasContainer.Container);
                    e.Handled = true;
                }
                else if (control.DataContext is IContainer container)
                {
                    ViewModel.CommandHandlerService.OpenContainer(container);
                }
                else if (control.DataContext is IElement element && element.GetParent() is IContainer parentContainer)
                {
                    Task.Run(async () =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await ViewModel.AppState.SelectedTab.OpenContainer(parentContainer);
                            await ViewModel.AppState.SelectedTab.SetCurrentSelectedItem(element);
                        });
                    });
                }
            }
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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