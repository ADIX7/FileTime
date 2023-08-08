using TerminalUI.Controls;

namespace TerminalUI;

public interface IEventLoop
{
    void Render();
    void AddViewToRender(IView view);
    void Run();
    void RequestRerender();
}