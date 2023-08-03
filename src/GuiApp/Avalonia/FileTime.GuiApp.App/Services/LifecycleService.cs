using FileTime.App.Core.Services;
using FileTime.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App.Services;

public class LifecycleService
{
    private readonly IEnumerable<IExitHandler> _exitHandlers;
    private readonly IEnumerable<IStartupHandler> _startupHandlers;
    private readonly ILogger<LifecycleService> _logger;

    public LifecycleService(
        IEnumerable<IStartupHandler> startupHandlers,
        IEnumerable<IExitHandler> exitHandlers,
        ILogger<LifecycleService> logger)
    {
        _exitHandlers = exitHandlers;
        _startupHandlers = startupHandlers;
        _logger = logger;
    }

    public async Task InitStartupHandlersAsync()
    {
        foreach (var startupHandler in _startupHandlers)
        {
            try
            {
                await startupHandler.InitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running startup handler {Handler}", startupHandler?.GetType().FullName);
            }
        }
    }

    public async Task ExitAsync()
    {
        var exitCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var exitHandlerTasks = _exitHandlers.Select(e =>
        {
            try
            {
                return e.ExitAsync(exitCancellation.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running exit handler {Handler}", e.GetType().FullName);
            }

            return Task.CompletedTask;
        });

        try
        {
            await Task.WhenAll(exitHandlerTasks).TimeoutAfter(10000);
        }
        catch
        {
            
        }

        exitCancellation.Cancel();
    }
}