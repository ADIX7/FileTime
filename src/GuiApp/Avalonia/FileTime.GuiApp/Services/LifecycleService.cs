using FileTime.App.Core.Services;

namespace FileTime.GuiApp.Services;

public class LifecycleService
{
    private readonly IEnumerable<IExitHandler> _exitHandlers;

    public LifecycleService(IEnumerable<IStartupHandler> startupHandlers, IEnumerable<IExitHandler> exitHandlers)
    {
        _exitHandlers = exitHandlers;
    }

    public async Task Exit()
    {
        foreach (var exitHandler in _exitHandlers)
        {
            await exitHandler.ExitAsync();
        }
    }
}