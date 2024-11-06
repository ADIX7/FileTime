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

    public static SignalBase<T> Debounce<T>(this SignalBase<T> signal, TimeSpan interval)
    {
        return new DebounceSignal<T>(signal, () => interval);
    }

    public static SignalBase<T> Debounce<T>(this SignalBase<T> signal, Func<TimeSpan> interval)
    {
        return new DebounceSignal<T>(signal, interval);
    }
}