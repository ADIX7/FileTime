using PropertyChanged.SourceGenerator;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public abstract partial class ContentView<T> : View<T>, IContentRenderer<T>
{
    private bool _placeholderRenderDone;
    [Notify] private RenderMethod _contentRendererMethod;
    private IView<T>? _content;

    public IView<T>? Content
    {
        get => _content;
        set
        {
            if (Equals(value, _content)) return;

            if (_content is not null)
            {
                RemoveChild(_content);
            }

            _content = value;

            if (_content is not null)
            {
                AddChild(_content);
            }

            OnPropertyChanged();
        }
    }

    protected ContentView()
    {
        _contentRendererMethod = DefaultContentRender;
        RerenderProperties.Add(nameof(Content));
        RerenderProperties.Add(nameof(ContentRendererMethod));
    }

    private bool DefaultContentRender(in RenderContext renderContext, Position position, Size size)
    {
        if (Content is null || !Content.IsVisible)
        {
            if (_placeholderRenderDone) return false;
            _placeholderRenderDone = true;
            RenderEmpty(renderContext, position, size);
            return true;
        }

        _placeholderRenderDone = false;
        return Content.Render(renderContext, position, size);
    }
}