using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTime.App.Core.UserCommand;
using FileTime.Core.Models;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger;

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
        InitializeComponent();
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
            if (control.DataContext is IHaveFullPath { Path: { } } hasFullPath)
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
}