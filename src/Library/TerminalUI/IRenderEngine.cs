using TerminalUI.Controls;

namespace TerminalUI;

public interface IRenderEngine
{
    void RequestRerender(IView view);
    void VisibilityChanged(IView view, bool newVisibility);
    void AddViewToPermanentRenderGroup(IView view);
    void Run();
}