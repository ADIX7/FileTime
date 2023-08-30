using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.App.CloudDrives;
using FileTime.GuiApp.App.Services;
using FileTime.GuiApp.App.Settings;
using FileTime.GuiApp.App.ViewModels;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App.Views;

public partial class MainWindow : Window, IUiAccessor
{
    private readonly Action? _initializer;
    private ILogger<MainWindow>? _logger;
    private IModalService? _modalService;
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

    private async void OnWindowOpened(object sender, EventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel && !Design.IsDesignMode)
            {
                await Task.Delay(100);
                _initializer?.Invoke();

                _logger = DI.ServiceProvider.GetService<ILogger<MainWindow>>();
                _modalService = DI.ServiceProvider.GetRequiredService<IModalService>();
                DI.ServiceProvider.GetRequiredService<SystemClipboardService>().UiAccessor = this;

                ReadInputContainer.PropertyChanged += ReadInputContainerOnPropertyChanged;

                _logger?.LogInformation(
                    $"{nameof(MainWindow)} opened, starting {nameof(MainWindowViewModel)} initialization...");

                try
                {
                    var viewModel = DI.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    viewModel.FocusDefaultElement = () => Focus();
                    viewModel.ShowWindow = Activate;
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
        catch
        {
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_modalService!.OpenModals.Count > 0) return;
        ViewModel?.ProcessKeyDown(e);
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
            if (control.DataContext is FullName p)
            {
                path = p;
            }
            else if (control.DataContext is RootDriveInfo {Path: { } rootDriveInfoPath})
            {
                path = rootDriveInfoPath;
            }
            else if (control.DataContext is PlaceInfo {Path: { } placeInfoPath})
            {
                path = placeInfoPath;
            }
            else if (control.DataContext is CloudDrive {Path: { } cloudDrivePath})
            {
                var timelessContentProvider = DI.ServiceProvider.GetRequiredService<ITimelessContentProvider>();
                path = await timelessContentProvider.GetFullNameByNativePathAsync(cloudDrivePath);
            }

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
        var inputViewModel = ViewModel!.DialogService.ReadInput.Value;
        if (e.Key == Key.Escape)
        {
            inputViewModel?.Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            inputViewModel?.Process();
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

    private void SettingsButtonClicked(object? sender, PointerPressedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            DataContext = DI.ServiceProvider.GetRequiredService<ISettingsViewModel>()
        };
        settingsWindow.ShowDialog(this);
    }
}