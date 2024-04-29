namespace Signal;

public static class Helpers
{
    public static SignalBase<TResult> CombineLatest<T1, T2, TResult>(SignalBase<T1> signal1,
        SignalBase<T2> signal2, Func<T1, T2, TResult> combine)
    {
        return new CombineLatestSignal<T1, T2, TResult>(signal1, signal2, combine);
    }
    public static SignalBase<TResult> CombineLatest<T1, T2, TResult>(SignalBase<T1> signal1,
        SignalBase<T2> signal2, Func<T1, T2, Task<TResult>> combine)
    {
        return new CombineLatestSignal<T1, T2, TResult>(signal1, signal2, combine);
    }
}