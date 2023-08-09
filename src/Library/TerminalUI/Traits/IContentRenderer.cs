using TerminalUI.Controls;
using TerminalUI.Models;

namespace TerminalUI.Traits;

public interface IContentRenderer
{
    IView? Content { get; set; }
    Action<Position, Size> ContentRendererMethod { get; set; }
}