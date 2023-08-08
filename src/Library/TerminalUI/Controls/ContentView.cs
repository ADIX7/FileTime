using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public abstract class ContentView<T>: View<T>, IContentRenderer
{
    protected ContentView()
    {
        ContentRendererMethod = DefaultContentRender;
    }
    public IView? Content { get; set; }
    public Action<Position> ContentRendererMethod { get; set; }

    private void DefaultContentRender(Position position) => Content?.Render(position);
}