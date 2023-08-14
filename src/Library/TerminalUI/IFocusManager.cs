using GeneralInputKey;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public interface IFocusManager
{
    void SetFocus(IFocusable focusable);
    void UnFocus(IFocusable focusable);
    IFocusable? Focused { get; }
    void HandleKeyInput(GeneralKeyEventArgs keyEventArgs);
    void FocusFirstElement(IView view, IView? from = null);
    void FocusLastElement(IView view, IView? from = null);
}