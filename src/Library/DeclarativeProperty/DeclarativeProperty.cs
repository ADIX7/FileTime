namespace DeclarativeProperty;

public static class DeclarativePropertyHelpers
{
    public static CombineLatestProperty<T1, T2, TResult> CombineLatest<T1, T2, TResult>(
        IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        Func<T1, T2, Task<TResult>> func,
        Action<TResult?>? setValueHook = null)
        => new(prop1, prop2, func, setValueHook);
}

public sealed class DeclarativeProperty<T> : DeclarativePropertyBase<T>
{
    public DeclarativeProperty(Action<T?>? setValueHook = null) : base(setValueHook)
    {
    }

    public DeclarativeProperty(T initialValue, Action<T?>? setValueHook = null) : base(initialValue, setValueHook)
    {

    }

    public async Task SetValue(T newValue, CancellationToken cancellationToken = default)
        => await SetNewValueAsync(newValue, cancellationToken);
}