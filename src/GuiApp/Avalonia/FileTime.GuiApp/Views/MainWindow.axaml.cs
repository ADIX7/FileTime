using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.Services;
using FileTime.GuiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Views;

public partial class MainWindow : Window, IUiAccessor
{
    private readonly Action? _initializer;
    private ILogger<MainWindow>? _logger;
    private IModalService? _modalService;
    private IReadOnlyCollection<IModalViewModel>? _openModals;
    private ReadInputsViewModel? _inputViewModel;
    private IDisposable? _inputViewModelSubscription;
    private bool _isShuttingDown;
    private bool _shutdownCompleted;
    private readonly object _isClosingLock = new();

    public MainWindowViewModel? ViewModel
    {
        get => DataContext as MainWindowViewModel;
        set
        {
            if (value != DataContext)
            {
                DataContext = value;
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            DataContext = new MainWindowDesignViewModel();
        }
    }

    public MainWindow(Action initializer) : this()
    {
        _initializer = initializer;
    }

    private void OnWindowOpened(object sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel && !Design.IsDesignMode)
        {
            _initializer?.Invoke();

            _logger = DI.ServiceProvider.GetService<ILogger<MainWindow>>();
            _modalService = DI.ServiceProvider.GetRequiredService<IModalService>();
            _modalService.OpenModals.ToCollection().Subscribe(m => _openModals = m);
            DI.ServiceProvider.GetRequiredService<Services.SystemClipboardService>().UiAccessor = this;

            ReadInputContainer.PropertyChanged += ReadInputContainerOnPropertyChanged;
            DataContextChanged += (_, _) =>
            {
                if (DataContext is not MainWindowViewModel mainWindowViewModel) return;

                _inputViewModelSubscription?.Dispose();
                _inputViewModelSubscription = mainWindowViewModel.DialogService.ReadInput.Subscribe(
                    inputViewModel => _inputViewModel = inputViewModel
                );
            };

            _logger?.LogInformation(
                $"{nameof(MainWindow)} opened, starting {nameof(MainWindowViewModel)} initialization...");

            try
            {
                var viewModel = DI.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                viewModel.FocusDefaultElement = () => Focus();
                ViewModel = viewModel;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing {ViewModel}", nameof(MainWindowViewModel));
                if (DataContext is IMainWindowViewModelBase mainWindowViewModelBase)
                {
                    mainWindowViewModelBase.FatalError.SetValueSafe(
                        $"Error initializing {nameof(MainWindowViewModel)}: " + ex.Message
                    );
                }
            }
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if ((_openModals?.Count ?? 0) > 0) return;
        ViewModel?.ProcessKeyDown(e.Key, e.KeyModifiers, h => e.Handled = h);
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

    private async void OnHasContainerPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!e.Handled
            && ViewModel != null
            && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            && sender is StyledElement control)
        {
            FullName? path = null;
            if (control.DataContext is IHaveFullPath {Path: { }} hasFullPath)
            {
                path = hasFullPath.Path;
            }
            else if (control.DataContext is FullName p)
            {
                path = p;
            }
            /*else if (control.DataContext is IElement element && element.GetParent() is IContainer parentContainer)
            {
                Task.Run(async () =>
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await ViewModel.AppState.SelectedTab.OpenContainer(parentContainer);
                        await ViewModel.AppState.SelectedTab.SetCurrentSelectedItem(element);
                    });
                });
            }*/

            if (path is null) return;

            await ViewModel.OpenContainerByFullName(path);
            e.Handled = true;
        }
    }

    private async void ReadInputContainerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ReadInputContainer.IsVisible) && ReadInputContainer.IsVisible)
        {
            await Task.Delay(100);
            var inputElement = InputList
                .GetVisualDescendants()
                .OfType<Control>()
                .FirstOrDefault(i => i.Tag as string == "InputItem" && i.IsVisible);
            inputElement?.Focus();
        }
    }


    private void Window_OnClosed(object? sender, EventArgs e)
    {
    }

    private void InputList_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _inputViewModel?.Cancel();
            _inputViewModel = null;
        }
        else if (e.Key == Key.Enter)
        {
            _inputViewModel?.Process();
            _inputViewModel = null;
        }
    }

    public TopLevel? GetTopLevel() => GetTopLevel(this);

    public async Task InvokeOnUIThread(Func<Task> func) => await Dispatcher.UIThread.InvokeAsync(func);

    public async Task<T> InvokeOnUIThread<T>(Func<Task<T>> func) => await Dispatcher.UIThread.InvokeAsync(func);

    private async void Child_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e is {Handled: false, ClickCount: 2}
            && ViewModel != null
            && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            && sender is StyledElement {DataContext: IItemViewModel itemViewModel})
        {
            try
            {
                await ViewModel.RunOrOpenItem(itemViewModel);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error while opening item {Item}",
                    itemViewModel.BaseItem?.FullName?.Path ?? itemViewModel.DisplayNameText
                );
            }
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        lock (_isClosingLock)
        {
            if (_isShuttingDown)
            {
                e.Cancel = true;
                return;
            }

            if (_shutdownCompleted)
            {
                return;
            }

            _isShuttingDown = true;
            e.Cancel = true;

            var vm = ViewModel;
            var exitVm = new MainWindowLoadingViewModel();
            exitVm.Title.SetValueSafe("Shutting down...");
            DataContext = exitVm;

            Task.Run(async () =>
            {
                await Task.Delay(200);
                try
                {
                    if (vm is not null)
                    {
                        await vm.OnExit();
                    }
                }
                catch
                {
                }

                lock (_isClosingLock)
                {
                    _isShuttingDown = false;
                    _shutdownCompleted = true;
                }

                Dispatcher.UIThread.Invoke(Close);
            });
        }
    }
}