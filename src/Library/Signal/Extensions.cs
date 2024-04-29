namespace Signal;

public static class Extensions
{
    public static SignalBase<TResult> Map<T, TResult>(this SignalBase<T> signal, Func<T, TResult> map)
    {
        return new MapSignal<T, TResult>(signal, map);
    }
    public static SignalBase<TResult> Map<T, TResult>(this SignalBase<T> signal, Func<T, Task<TResult>> map)
    {
        return new MapSignal<T, TResult>(signal, map);
    }
}