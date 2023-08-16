using TerminalUI.Controls;
using TerminalUI.Models;
using TerminalUI.TextFormat;
using TerminalUI.Traits;

namespace TerminalUI;

public class RenderEngine : IRenderEngine
{
    private readonly IApplicationContext _applicationContext;
    private readonly IEventLoop _eventLoop;
    private readonly object _lock = new();
    private readonly List<IView> _permanentViewsToRender = new();
    private readonly List<IView> _forcedTemporaryViewsToRender = new();
    private bool _rerenderRequested = true;
    private bool _lastCursorVisible;

    public RenderEngine(IApplicationContext applicationContext, IEventLoop eventLoop)
    {
        _applicationContext = applicationContext;
        _eventLoop = eventLoop;

        _eventLoop.AddToPermanentQueue(Render);
    }

    public void RequestRerender(IView view) => RequestRerender();

    public void VisibilityChanged(IView view)
    {
        IVisibilityChangeHandler? visibilityChangeHandler = null;
        var parent = view.VisualParent;
        while (parent?.VisualParent != null)
        {
            if (parent is IVisibilityChangeHandler v)
            {
                visibilityChangeHandler = v;
                break;
            }

            parent = parent.VisualParent;
        }

        if (visibilityChangeHandler is null)
        {
            AddViewToForcedTemporaryRenderGroup(parent ?? view);
        }
        else
        {
            visibilityChangeHandler.ChildVisibilityChanged(view);
        }
    }

    public void Run() => _eventLoop.Run();

    public void RequestRerender()
    {
        lock (_lock)
        {
            _rerenderRequested = true;
        }
    }

    private void Render()
    {
        List<IView> permanentViewsToRender;
        List<IView> forcedTemporaryViewsToRender;
        lock (_lock)
        {
            if (!_rerenderRequested) return;
            _rerenderRequested = false;
            permanentViewsToRender = _permanentViewsToRender.ToList();
            forcedTemporaryViewsToRender = _forcedTemporaryViewsToRender.ToList();
            _forcedTemporaryViewsToRender.Clear();
        }

        var driver = _applicationContext.ConsoleDriver;
        var initialPosition = new Position(0, 0);
        var size = driver.GetWindowSize();

        RenderViews(
            forcedTemporaryViewsToRender,
            new RenderContext(
                driver,
                true,
                null,
                null,
                new RenderStatistics(),
                new TextFormatContext(driver.SupportsAnsiEscapeSequence)
            ),
            initialPosition,
            size);

        RenderViews(
            permanentViewsToRender,
            new RenderContext(
                driver,
                false,
                null,
                null,
                new RenderStatistics(),
                new TextFormatContext(driver.SupportsAnsiEscapeSequence)
            ),
            initialPosition,
            size);

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

    private void RenderViews(List<IView> views, in RenderContext renderContext, Position position, Size size)
    {
        foreach (var view in views)
        {
            view.Attached = true;
            view.GetRequestedSize();
            view.Render(renderContext, position, size);
        }
    }

    public void AddViewToPermanentRenderGroup(IView view)
    {
        lock (_lock)
        {
            _permanentViewsToRender.Add(view);
        }
    }

    public void AddViewToForcedTemporaryRenderGroup(IView view)
    {
        lock (_lock)
        {
            _forcedTemporaryViewsToRender.Add(view);
        }
    }
}