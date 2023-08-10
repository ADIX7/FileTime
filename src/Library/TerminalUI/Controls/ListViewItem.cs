using TerminalUI.Models;

namespace TerminalUI.Controls;

public class ListViewItem<T> : ContentView<T>
{
    protected override Size CalculateSize()
    {
        if (Content is null) return new Size(0, 0);
        return Content.GetRequestedSize();
    }

    protected override bool DefaultRenderer(RenderContext renderContext, Position position, Size size)
    {
        if (ContentRendererMethod is null)
        {
            throw new NullReferenceException(
                nameof(ContentRendererMethod)
                + " is null, cannot render content of "
                + Content?.GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        return ContentRendererMethod(renderContext, position, size);
    }
}