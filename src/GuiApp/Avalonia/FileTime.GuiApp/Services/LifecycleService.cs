using FileTime.App.Core.Services;

namespace FileTime.GuiApp.Services;

public class LifecycleService
{
    private readonly IEnumerable<IExitHandler> _exitHandlers;
    private readonly IEnumerable<IStartupHandler> _startupHandlers;

    public LifecycleService(IEnumerable<IStartupHandler> startupHandlers, IEnumerable<IExitHandler> exitHandlers)
    {
        _exitHandlers = exitHandlers;
        _startupHandlers = startupHandlers;
    }

    public async Task InitStartupHandlersAsync()
    {
        foreach (var startupHandler in _startupHandlers)
        {
            await startupHandler.InitAsync();
        }
    }

    public async Task ExitAsync()
    {
        foreach (var exitHandler in _exitHandlers)
        {
            await exitHandler.ExitAsync();
        }
    }
}