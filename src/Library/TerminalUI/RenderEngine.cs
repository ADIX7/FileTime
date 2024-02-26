using Microsoft.Extensions.Logging;
using TerminalUI.Controls;
using TerminalUI.Models;
using TerminalUI.Styling;
using TerminalUI.TextFormat;
using TerminalUI.Traits;

namespace TerminalUI;

public class RenderEngine : IRenderEngine
{
    private readonly IApplicationContext _applicationContext;
    private readonly IEventLoop _eventLoop;
    private readonly ILogger<RenderEngine> _logger;
    private readonly object _lock = new();
    private readonly List<IView> _permanentViewsToRender = new();
    private readonly List<IView> _forcedTemporaryViewsToRender = new();
    private bool _rerenderRequested = true;
    private bool _lastCursorVisible;
    private bool _forceRerenderAll;
    private bool[,]? _updatedCells;
    private bool[,]? _filledCells;
    private bool[,]? _lastFilledCells;
    private ITheme? _lastTheme;

    public RenderEngine(IApplicationContext applicationContext, IEventLoop eventLoop, ILogger<RenderEngine> logger)
    {
        _applicationContext = applicationContext;
        _eventLoop = eventLoop;
        _logger = logger;

        _eventLoop.AddToPermanentQueue(Render);
        _eventLoop.AddInitializer(() =>
        {
            _applicationContext.ConsoleDriver.ThreadId = _eventLoop.ThreadId;
            _applicationContext.ConsoleDriver.EnterRestrictedMode();
        });
        _eventLoop.AddFinalizer(() => { _applicationContext.ConsoleDriver.ExitRestrictedMode(); });
    }

    public void RequestRerender(IView view) => RequestRerender();

    public void VisibilityChanged(IView view, bool newVisibility)
    {
        if (!newVisibility)
        {
            _forceRerenderAll = true;
            return;
        }

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
            return;
        }

        visibilityChangeHandler.ChildVisibilityChanged(view);
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
        bool forceRerenderAll;
        lock (_lock)
        {
            if (!_rerenderRequested) return;
            _rerenderRequested = false;
            permanentViewsToRender = _permanentViewsToRender.ToList();
            forcedTemporaryViewsToRender = _forcedTemporaryViewsToRender.ToList();
            _forcedTemporaryViewsToRender.Clear();

            forceRerenderAll = _forceRerenderAll;
            _forceRerenderAll = false;
        }

        if (_applicationContext.Theme != _lastTheme)
        {
            _lastTheme = _applicationContext.Theme;
            forceRerenderAll = true;
        }

        var driver = _applicationContext.ConsoleDriver;
        var initialPosition = new Position(0, 0);
        var size = driver.GetWindowSize();

        //TODO: this could be stack allocated when sizes are small
        if (_updatedCells is null
            || _updatedCells.GetLength(0) != size.Width
            || _updatedCells.GetLength(1) != size.Height)
        {
            _updatedCells = new bool[size.Width, size.Height];
        }
        else
        {
            ClearArray2D(_updatedCells);
        }

        if (!forceRerenderAll)
        {
            RenderViews(
                forcedTemporaryViewsToRender,
                new RenderContext(
                    driver,
                    true,
                    null,
                    null,
                    new RenderStatistics(),
                    new TextFormatContext(driver.SupportsAnsiEscapeSequence),
                    _updatedCells
                ),
                initialPosition,
                size);
        }

        RenderViews(
            permanentViewsToRender,
            new RenderContext(
                driver,
                forceRerenderAll,
                null,
                null,
                new RenderStatistics(),
                new TextFormatContext(driver.SupportsAnsiEscapeSequence),
                _updatedCells
            ),
            initialPosition,
            size);

        if (_lastFilledCells is not null
            && _lastFilledCells.GetLength(0) == size.Width
            && _lastFilledCells.GetLength(1) == size.Height)
        {
            Array2DHelper.CombineArray2Ds(
                _updatedCells,
                _lastFilledCells,
                new Position(0, 0),
                _updatedCells,
                (a, b) => (a ?? false) || (b ?? false)
            );
        }

        if (_filledCells is null
            || _filledCells.GetLength(0) != size.Width
            || _filledCells.GetLength(1) != size.Height)
        {
            _filledCells = new bool[size.Width, size.Height];
        }
        else
        {
            ClearArray2D(_filledCells);
        }

        driver.ResetStyle();
        Array2DHelper.RenderEmpty(
            driver,
            _updatedCells,
            _filledCells,
            _applicationContext.EmptyCharacter,
            initialPosition,
            size);

        (_lastFilledCells, _filledCells) = (_filledCells, _lastFilledCells);

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
            try
            {
                view.Attached = true;
                view.GetRequestedSize();
                view.Render(renderContext, position, size);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while rendering view");
            }
        }
    }

    private void ClearArray2D<T>(T[,] array, T defaultValue = default!)
    {
        var maxX = array.GetLength(0);
        var maxY = array.GetLength(1);

        for (var x = 0; x < maxX; x++)
        {
            for (var y = 0; y < maxY; y++)
            {
                array[x, y] = defaultValue;
            }
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