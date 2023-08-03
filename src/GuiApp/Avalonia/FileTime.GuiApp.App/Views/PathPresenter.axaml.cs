using Avalonia.Controls;
using Avalonia.Input;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App.Views;

public partial class PathPresenter : UserControl
{
    private readonly Lazy<ILogger<PathPresenter>> _logger;

    public PathPresenter()
    {
        InitializeComponent();
        _logger = new Lazy<ILogger<PathPresenter>>(
            () => DI.ServiceProvider.GetRequiredService<ILogger<PathPresenter>>()
        );
    }

    private async void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
                && DataContext is string fullPath
                && sender is TextBlock textBlock)
            {
                var pathPart = textBlock.Text;
                var path = fullPath[..(fullPath.IndexOf(pathPart) + pathPart.Length)];
                var timelessContentProvider = DI.ServiceProvider.GetRequiredService<ITimelessContentProvider>();
                var userCommandHandlerService = DI.ServiceProvider.GetRequiredService<IUserCommandHandlerService>();
                await userCommandHandlerService.HandleCommandAsync(
                    new OpenContainerCommand(
                        new AbsolutePath(
                            timelessContentProvider,
                            PointInTime.Present,
                            new FullName(path),
                            AbsolutePathType.Container)
                    )
                );
            }
        }
        catch (Exception exception)
        {
            _logger.Value.LogError(exception, "Failed to open container");
        }
    }
}