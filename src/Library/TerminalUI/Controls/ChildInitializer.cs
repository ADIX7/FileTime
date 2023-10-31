using System.Collections;
using System.Linq.Expressions;
using TerminalUI.Extensions;

namespace TerminalUI.Controls;

public record ChildWithDataContextMapper<TSourceDataContext, TTargetDataContext>(IView<TTargetDataContext> Child, Func<TSourceDataContext?, TTargetDataContext?> DataContextMapper);
public record ChildWithDataContextBinding<TSourceDataContext, TTargetDataContext>(IView<TTargetDataContext> Child, Expression<Func<TSourceDataContext?, TTargetDataContext?>> DataContextExpression);

public class ChildInitializer<T>(IChildContainer<T> childContainer) : IEnumerable<IView>
{
    public void Add(IView<T> item) => childContainer.AddChild(item);

    public void Add<TDataContext>(ChildWithDataContextMapper<T, TDataContext> item)
        => childContainer.AddChild(item.Child, item.DataContextMapper);
    
    public void Add<TDataContext>(ChildWithDataContextBinding<T, TDataContext> item)
    {
        item.Child.Bind(
            childContainer,
            item.DataContextExpression,
            c => c.DataContext
        );
        childContainer.AddChild(item.Child);
    }

    public IEnumerator<IView> GetEnumerator() => childContainer.Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}