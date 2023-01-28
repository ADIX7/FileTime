using FileTime.App.Core.Services;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Services;

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
        foreach (var exitHandler in _exitHandlers)
        {
            try
            {
                await exitHandler.ExitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running exit handler {Handler}", exitHandler?.GetType().FullName);
            }
        }

        foreach (var disposable in
                 _startupHandlers
                     .OfType<IDisposable>()
                     .Concat(
                         _exitHandlers.OfType<IDisposable>()
                     )
                )
        {
            disposable.Dispose();
        }
    }
}