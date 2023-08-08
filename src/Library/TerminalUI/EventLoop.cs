using TerminalUI.Controls;
using TerminalUI.Models;

namespace TerminalUI;

public class EventLoop : IEventLoop
{
    private readonly IApplicationContext _applicationContext;
    private readonly object _lock = new();
    private readonly List<IView> _viewsToRender = new();
    private bool _rerenderRequested;

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

        foreach (var view in viewsToRender)
        {
            view.Render(new Position(0, 0));
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