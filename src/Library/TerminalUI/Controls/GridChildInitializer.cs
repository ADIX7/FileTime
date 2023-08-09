using System.Collections;

namespace TerminalUI.Controls;

public record ChildWithDataContextMapper<TSourceDataContext, TTargetDataContext>(IView<TTargetDataContext> Child, Func<TSourceDataContext?, TTargetDataContext?> DataContextMapper);

public class GridChildInitializer<T> : IEnumerable<IView>
{
    private readonly Grid<T> _grid;

    public GridChildInitializer(Grid<T> grid)
    {
        _grid = grid;
    }

    public void Add(IView<T> item) => _grid.AddChild(item);

    public void Add<TDataContext>(ChildWithDataContextMapper<T, TDataContext> item)
        => _grid.AddChild(item.Child, item.DataContextMapper);

    public IEnumerator<IView> GetEnumerator() => _grid.Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}