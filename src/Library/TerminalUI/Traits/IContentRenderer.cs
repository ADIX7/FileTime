using TerminalUI.Controls;

namespace TerminalUI.Traits;

public interface IContentRenderer
{
    IView? Content { get; set; }
    Action ContentRendererMethod { get; set; }
}