using TerminalUI.Traits;

namespace TerminalUI;

public interface IFocusManager
{
    void SetFocus(IFocusable focusable);
    void UnFocus(IFocusable focusable);
    IFocusable? Focused { get; }
}