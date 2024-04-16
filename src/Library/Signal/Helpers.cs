namespace Signal;

public static class Helpers
{
    public static IReadOnlySignal<TResult> CombineLatest<T1, T2, TResult>(IReadOnlySignal<T1> signal1,
        IReadOnlySignal<T2> signal2, Func<T1, T2, TResult> combine)
    {
        return new CombineLatestSignal<T1, T2, TResult>(signal1, signal2, combine);
    }
    public static IReadOnlySignal<TResult> CombineLatest<T1, T2, TResult>(IReadOnlySignal<T1> signal1,
        IReadOnlySignal<T2> signal2, Func<T1, T2, Task<TResult>> combine)
    {
        return new CombineLatestSignal<T1, T2, TResult>(signal1, signal2, combine);
    }
}