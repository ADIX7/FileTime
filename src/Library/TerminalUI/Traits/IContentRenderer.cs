using TerminalUI.Controls;

namespace TerminalUI.Traits;

public interface IContentRenderer<T>
{
    IView<T>? Content { get; set; }
    RenderMethod ContentRendererMethod { get; set; }
}