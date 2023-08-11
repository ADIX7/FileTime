using TerminalUI.Controls;
using TerminalUI.Models;

namespace TerminalUI;

public class EventLoop : IEventLoop
{
    private readonly IApplicationContext _applicationContext;
    private readonly object _lock = new();
    private readonly List<IView> _viewsToRender = new();
    private bool _rerenderRequested;
    private bool _lastCursorVisible;

    public EventLoop(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public void Run()
    {
        _applicationContext.IsRunning = true;
        _rerenderRequested = true;
        while (_applicationContext.IsRunning)
        {
            Render();
            Thread.Sleep(10);
        }
    }

    public void RequestRerender()
    {
        lock (_lock)
        {
            _rerenderRequested = true;
        }
    }

    public void Render()
    {
        List<IView> viewsToRender;
        lock (_lock)
        {
            if (!_rerenderRequested) return;
            _rerenderRequested = false;
            viewsToRender = _viewsToRender.ToList();
        }

        var driver = _applicationContext.ConsoleDriver;
        var size = driver.GetWindowSize();
        var renderContext = new RenderContext(
            driver,
            false,
            null,
            null
        );
        foreach (var view in viewsToRender)
        {
            view.Attached = true;
            view.GetRequestedSize();
            view.Render(renderContext, new Position(0, 0), size);
        }

        if (_applicationContext.FocusManager.Focused is { } focused)
        {
            focused.SetCursorPosition(driver);
            if (!_lastCursorVisible)
            {
                driver.SetCursorVisible(true);
                _lastCursorVisible = true;
            }
        }
        else if (_lastCursorVisible)
        {
            driver.SetCursorVisible(false);
            _lastCursorVisible = false;
        }
    }

    public void AddViewToRender(IView view)
    {
        lock (_lock)
        {
            _viewsToRender.Add(view);
        }
    }
}