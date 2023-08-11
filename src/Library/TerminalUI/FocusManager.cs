using TerminalUI.Traits;

namespace TerminalUI;

public class FocusManager : IFocusManager
{
    private IFocusable? _focused;

    public IFocusable? Focused
    {
        get
        {
            if (_focused is not null && !_focused.IsVisible)
            {
                _focused = null;
            }

            return _focused;
        }

        private set => _focused = value;
    }

    public void SetFocus(IFocusable focusable) => Focused = focusable;

    public void UnFocus(IFocusable focusable)
    {
        if (Focused == focusable)
            Focused = null;
    }
}