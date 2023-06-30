using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using DynamicData;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger;
    private readonly IModalService _modalService;
    private IReadOnlyCollection<IModalViewModel>? _openModals;

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
        _logger = DI.ServiceProvider.GetService<ILogger<MainWindow>>();
        _logger?.LogInformation($"Starting {nameof(MainWindow)} initialization...");
        _modalService = DI.ServiceProvider.GetRequiredService<IModalService>();
        _modalService.OpenModals.ToCollection().Subscribe(m => _openModals = m);
        DI.ServiceProvider.GetRequiredService<Services.SystemClipboardService>().TopLevel = GetTopLevel(this);
        InitializeComponent();

        ReadInputContainer.PropertyChanged += ReadInputContainerOnPropertyChanged;
    }

    private void OnWindowOpened(object sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel)
        {
            _logger?.LogInformation(
                $"{nameof(MainWindow)} opened, starting {nameof(MainWindowViewModel)} initialization...");
            ViewModel = DI.ServiceProvider.GetRequiredService<MainWindowViewModel>();
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

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        var vm = ViewModel;
        Task.Run(() => vm?.OnExit()).Wait();
    }
}