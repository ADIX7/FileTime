using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class ListViewItem<T, TParentDataContext> : ContentView<T>
{
    public ListView<TParentDataContext, T> Parent { get; }
    [Notify] private bool _isSelected;

    public ListViewItem(ListView<TParentDataContext, T> parent)
    {
        Parent = parent;
        
        RerenderProperties.Add(nameof(IsSelected));
    }

    protected override Size CalculateSize()
    {
        if (Content is null || !Content.IsVisible) return new Size(0, 0);
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