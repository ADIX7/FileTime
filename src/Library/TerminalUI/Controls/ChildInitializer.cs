using System.Collections;
using System.Linq.Expressions;
using TerminalUI.Extensions;

namespace TerminalUI.Controls;

public record ChildWithDataContextMapper<TSourceDataContext, TTargetDataContext>(IView<TTargetDataContext> Child, Func<TSourceDataContext?, TTargetDataContext?> DataContextMapper);
public record ChildWithDataContextBinding<TSourceDataContext, TTargetDataContext>(IView<TTargetDataContext> Child, Expression<Func<TSourceDataContext?, TTargetDataContext?>> DataContextExpression);

public class ChildInitializer<T> : IEnumerable<IView>
{
    private readonly IChildContainer<T> _childContainer;

    public ChildInitializer(IChildContainer<T> childContainer)
    {
        _childContainer = childContainer;
    }

    public void Add(IView<T> item) => _childContainer.AddChild(item);

    public void Add<TDataContext>(ChildWithDataContextMapper<T, TDataContext> item)
        => _childContainer.AddChild(item.Child, item.DataContextMapper);
    
    public void Add<TDataContext>(ChildWithDataContextBinding<T, TDataContext> item)
    {
        item.Child.Bind(
            _childContainer,
            item.DataContextExpression,
            c => c.DataContext
        );
        _childContainer.AddChild(item.Child);
    }

    public IEnumerator<IView> GetEnumerator() => _childContainer.Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}