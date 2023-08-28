using GeneralInputKey;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public partial class Button<T> : ContentView<Button<T>, T>, IFocusable 
{
    private record RenderState(IColor? Foreground, IColor? Background);

    private Position? _cursorRenderPosition;
    private RenderState? _lastRenderState;

    [Notify] private Position? _cursorPosition;
    private List<Action<Button<T>>> _clickHandlers = new();

    public Button()
    {
        RerenderProperties.Add(nameof(CursorPosition));
    }

    public void Focus()
        => ApplicationContext?.FocusManager.SetFocus(this);

    public void UnFocus()
        => ApplicationContext?.FocusManager.UnFocus(this);

    public void SetCursorPosition(IConsoleDriver consoleDriver)
    {
        if (_cursorRenderPosition is null) return;
        consoleDriver.SetCursorPosition(_cursorRenderPosition.Value);
    }

    protected override Size CalculateSize()
    {
        if (Content is null || !Content.IsVisible) return new Size(0, 0);

        return Content.GetRequestedSize();
    }

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        if (ContentRendererMethod is null)
        {
            throw new NullReferenceException(
                nameof(ContentRendererMethod)
                + " is null, cannot render content of "
                + Content?.GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        var backgroundColor = Background ?? renderContext.Background;
        var foregroundColor = Foreground ?? renderContext.Foreground;

        var renderState = new RenderState(foregroundColor, backgroundColor);

        var forceRerender = !renderContext.ForceRerender && !NeedsRerender(renderState);
        _lastRenderState = renderState;

        var childRenderContext = renderContext with
        {
            Background = backgroundColor,
            Foreground = foregroundColor,
            ForceRerender = forceRerender
        };

        _cursorRenderPosition = position;

        return ContentRendererMethod(childRenderContext, position, size);
    }

    private bool NeedsRerender(RenderState renderState) => renderState != _lastRenderState;

    public override void HandleKeyInput(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key != Keys.Enter) return;

        keyEventArgs.Handled = true;
        foreach (var clickHandler in _clickHandlers)
        {
            clickHandler(this);
        }
    }

    public Button<T> WithClickHandler(Action<Button<T>> handler)
    {
        _clickHandlers.Add(handler);
        return this;
    }
}