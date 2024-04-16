namespace Signal;

public static class Extensions
{
    public static IReadOnlySignal<TResult> Map<T, TResult>(this IReadOnlySignal<T> signal, Func<T, TResult> map)
    {
        return new MapSignal<T, TResult>(signal, map);
    }
    public static IReadOnlySignal<TResult> Map<T, TResult>(this IReadOnlySignal<T> signal, Func<T, Task<TResult>> map)
    {
        return new MapSignal<T, TResult>(signal, map);
    }
}