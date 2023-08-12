using System.Diagnostics;
using GeneralInputKey;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

[DebuggerDisplay("Text = {Text}")]
public partial class TextBox<T> : View<T>, IFocusable, IDisplayView
{
    private record RenderState(
        string? Text,
        Position Position,
        Size Size,
        IColor? ForegroundColor,
        IColor? BackgroundColor
    );

    private readonly List<Action<GeneralKeyEventArgs>> _keyHandlers = new();

    private RenderState? _lastRenderState;
    private string _text = string.Empty;
    private List<string> _textLines;

    private Position? _cursorPosition;
    private Position _relativeCursorPosition = new(0, 0);

    [Notify] private bool _multiLine;
    public bool SetKeyHandledIfKnown { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value == _text) return;
            _text = MultiLine ? value : value.Split(Environment.NewLine)[0];

            _textLines = _text.Split(Environment.NewLine).ToList();
            UpdateRelativeCursorPosition();

            OnPropertyChanged();
        }
    }

    public TextBox()
    {
        _textLines = _text.Split(Environment.NewLine).ToList();
        RerenderProperties.Add(nameof(Text));
        RerenderProperties.Add(nameof(MultiLine));
    }

    private void UpdateTextField()
    {
        _text = string.Join(Environment.NewLine, _textLines);
        UpdateRelativeCursorPosition();
        OnPropertyChanged(nameof(Text));
    }

    private void UpdateRelativeCursorPosition()
    {
        if (_relativeCursorPosition.Y > _textLines.Count - 1)
            _relativeCursorPosition = _relativeCursorPosition with {Y = _textLines.Count - 1};

        if (_relativeCursorPosition.X > _textLines[_relativeCursorPosition.Y].Length)
            _relativeCursorPosition = _relativeCursorPosition with {X = _textLines[_relativeCursorPosition.Y].Length};
    }

    protected override Size CalculateSize() => new(Width ?? 10, Height ?? 1);

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        var foreground = Foreground ?? renderContext.Foreground;
        var background = Background ?? renderContext.Background;
        var renderStatus = new RenderState(
            Text,
            position,
            size,
            foreground,
            background);

        if (!renderContext.ForceRerender && !NeedsRerender(renderStatus)) return false;
        _lastRenderState = renderStatus;

        var driver = renderContext.ConsoleDriver;
        driver.ResetColor();
        if (foreground is not null)
        {
            driver.SetForegroundColor(foreground);
        }

        if (background is not null)
        {
            driver.SetBackgroundColor(background);
        }

        RenderEmpty(renderContext, position, size);
        RenderText(_textLines, driver, position, size);
        _cursorPosition = position + _relativeCursorPosition;

        return true;
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;

    public void Focus()
        => ApplicationContext?.FocusManager.SetFocus(this);

    public void UnFocus()
        => ApplicationContext?.FocusManager.UnFocus(this);

    public void SetCursorPosition(IConsoleDriver consoleDriver)
    {
        if (_cursorPosition is null) return;
        consoleDriver.SetCursorPosition(_cursorPosition.Value);
    }

    public void HandleKeyInput(GeneralKeyEventArgs keyEventArgs)
    {
        HandleKeyInputInternal(keyEventArgs);
        if (keyEventArgs.Handled)
        {
            ApplicationContext?.RenderEngine.RequestRerender(this);
        }
    }

    private void HandleKeyInputInternal(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return;

        if (HandleBackspace(keyEventArgs, out var known))
            return;

        if (!known && HandleDelete(keyEventArgs, out known))
            return;

        if (!known && HandleNavigation(keyEventArgs, out known))
            return;

        if (!known && ProcessKeyHandlers(keyEventArgs))
            return;

        if (!known
            && keyEventArgs.KeyChar != '\0'
            && keyEventArgs.KeyChar.ToString() is {Length: 1} keyString)
        {
            var y = _relativeCursorPosition.Y;
            var x = _relativeCursorPosition.X;
            _textLines[y] = _textLines[y][..x] + keyString + _textLines[y][x..];
            _relativeCursorPosition = _relativeCursorPosition with {X = x + 1};

            keyEventArgs.Handled = true;
            UpdateTextField();
        }
    }

    private bool HandleBackspace(GeneralKeyEventArgs keyEventArgs, out bool known)
    {
        if (keyEventArgs.Key != Keys.Backspace)
        {
            known = false;
            return false;
        }

        known = true;

        if (_relativeCursorPosition is {X: 0, Y: 0})
        {
            return keyEventArgs.Handled = SetKeyHandledIfKnown;
        }

        if (_relativeCursorPosition.X == 0)
        {
            var y = _relativeCursorPosition.Y;
            _textLines[y - 1] += _textLines[y];
            _textLines.RemoveAt(y);
            _relativeCursorPosition = new Position(Y: y - 1, X: _textLines[y - 1].Length);
        }
        else
        {
            var y = _relativeCursorPosition.Y;
            var x = _relativeCursorPosition.X;
            _textLines[y] = _textLines[y].Remove(x - 1, 1);
            _relativeCursorPosition = _relativeCursorPosition with {X = x - 1};
        }

        UpdateTextField();
        return keyEventArgs.Handled = true;
    }

    private bool HandleDelete(GeneralKeyEventArgs keyEventArgs, out bool known)
    {
        if (keyEventArgs.Key != Keys.Delete)
        {
            known = false;
            return false;
        }

        known = true;

        if (_relativeCursorPosition.Y == _textLines.Count - 1
            && _relativeCursorPosition.X == _textLines[_relativeCursorPosition.Y].Length)
        {
            return keyEventArgs.Handled = SetKeyHandledIfKnown;
        }

        if (_relativeCursorPosition.X == _textLines[_relativeCursorPosition.Y].Length)
        {
            var y = _relativeCursorPosition.Y;
            _textLines[y] += _textLines[y + 1];
            _textLines.RemoveAt(y + 1);
        }
        else
        {
            var y = _relativeCursorPosition.Y;
            var x = _relativeCursorPosition.X;
            _textLines[y] = _textLines[y].Remove(x, 1);
        }

        UpdateTextField();
        return keyEventArgs.Handled = true;
    }

    private bool HandleNavigation(GeneralKeyEventArgs keyEventArgs, out bool known)
    {
        if (keyEventArgs.Key == Keys.Left)
        {
            known = true;
            keyEventArgs.Handled = SetKeyHandledIfKnown;

            if (_relativeCursorPosition is {X: 0, Y: 0})
            {
                return keyEventArgs.Handled;
            }

            if (_relativeCursorPosition.X == 0)
            {
                var y = _relativeCursorPosition.Y - 1;
                _relativeCursorPosition = new Position(_textLines[y].Length, y);
            }
            else
            {
                _relativeCursorPosition = _relativeCursorPosition with {X = _relativeCursorPosition.X - 1};
            }

            return keyEventArgs.Handled = true;
        }
        else if (keyEventArgs.Key == Keys.Right)
        {
            known = true;
            keyEventArgs.Handled = SetKeyHandledIfKnown;

            if (_relativeCursorPosition.Y == _textLines.Count - 1
                && _relativeCursorPosition.X == _textLines[_relativeCursorPosition.Y].Length)
            {
                return keyEventArgs.Handled;
            }

            if (_relativeCursorPosition.X == _textLines[_relativeCursorPosition.Y].Length)
            {
                _relativeCursorPosition = new Position(0, _relativeCursorPosition.Y + 1);
            }
            else
            {
                _relativeCursorPosition = _relativeCursorPosition with {X = _relativeCursorPosition.X + 1};
            }

            return keyEventArgs.Handled = true;
        }


        known = false;
        return false;
    }

    private bool ProcessKeyHandlers(GeneralKeyEventArgs keyEventArgs)
    {
        foreach (var keyHandler in _keyHandlers)
        {
            keyHandler(keyEventArgs);
            if (keyEventArgs.Handled) return true;
        }

        return false;
    }

    public TextBox<T> WithKeyHandler(Action<GeneralKeyEventArgs> keyHandler)
    {
        _keyHandlers.Add(keyHandler);
        return this;
    }
}