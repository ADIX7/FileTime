using TerminalUI.Models;

namespace TerminalUI.Controls;

public class ListViewItem<T> : ContentView<T>
{
    public override Size GetRequestedSize()
    {
        if (Content is null) return new Size(0, 0);
        return Content.GetRequestedSize();
    }

    protected override void DefaultRenderer(Position position, Size size)
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

        ContentRendererMethod(position, size);
    }
}