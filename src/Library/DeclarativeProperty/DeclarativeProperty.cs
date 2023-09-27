namespace DeclarativeProperty;

public static class DeclarativePropertyHelpers
{
    public static CombineLatestProperty<T1, T2, TResult> CombineLatest<T1, T2, TResult>(
        IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        Func<T1, T2, Task<TResult>> func,
        Action<TResult>? setValueHook = null)
        => new(prop1, prop2, func, setValueHook);

    public static CombineLatestProperty<T1, T2, T3, TResult> CombineLatest<T1, T2, T3, TResult>(
        IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        IDeclarativeProperty<T3> prop3,
        Func<T1, T2, T3, Task<TResult>> func,
        Action<TResult>? setValueHook = null)
        => new(prop1, prop2, prop3, func!, setValueHook);
    
    public static MergeProperty<T> Merge<T>(params IDeclarativeProperty<T>[] props)
        => new(props);
}

public sealed class DeclarativeProperty<T> : DeclarativePropertyBase<T>
{
    public DeclarativeProperty(T initialValue, Action<T>? setValueHook = null) : base(initialValue, setValueHook)
    {
    }

    public async Task SetValue(T newValue, CancellationToken cancellationToken = default)
        => await SetNewValueAsync(newValue, cancellationToken);

    public void SetValueSafe(T newValue, CancellationToken cancellationToken = default)
    {
        SetNewValueSync(newValue, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        Task.Run(async () =>
        {
            try
            {
                await NotifySubscribersAsync(newValue, cancellationToken);
            }
            catch
            {
                // ignored
            }
        });
    }
}