using TerminalUI.Controls;

namespace TerminalUI.Traits;

public interface IVisibilityChangeHandler
{
    void ChildVisibilityChanged(IView child);
}