using TerminalUI.Controls;

namespace TerminalUI.Extensions;

public static class ViewExtensions
{
    public static T? GetExtension<T>(this IView view)
        => (T?) view.Extensions.FirstOrDefault(e => e is T);

    public static ChildWithDataContextMapper<TSourceDataContext, TTargetDataContext> WithDataContextMapper<TSourceDataContext, TTargetDataContext>(
        this IView<TTargetDataContext> view,
        Func<TSourceDataContext?, TTargetDataContext?> dataContextMapper)
        => new(view, dataContextMapper);

    public static TView Setup<TView>(this TView view, Action<TView> action)
    {
        action(view);
        return view;
    }
}