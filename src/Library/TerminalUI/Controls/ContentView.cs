using TerminalUI.Traits;

namespace TerminalUI.Controls;

public abstract class ContentView<T>: View<T>, IContentRenderer
{
    protected ContentView()
    {
        ContentRendererMethod = DefaultContentRender;
    }
    public IView? Content { get; set; }
    public Action ContentRendererMethod { get; set; }

    private void DefaultContentRender() => Content?.Render();
}