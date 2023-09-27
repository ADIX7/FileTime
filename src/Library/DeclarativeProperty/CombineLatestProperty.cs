namespace DeclarativeProperty;

public sealed class CombineLatestProperty<T1, T2, TResult> : DeclarativePropertyBase<TResult>
{
    private readonly Func<T1, T2, Task<TResult>> _func;
    private T1 _value1;
    private T2 _value2;

    public CombineLatestProperty(
        IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        Func<T1, T2, Task<TResult>> func,
        Action<TResult>? setValueHook = null) : base(default!, setValueHook)
    {
        ArgumentNullException.ThrowIfNull(prop1);
        ArgumentNullException.ThrowIfNull(prop2);

        _func = func;

        _value1 = prop1.Value;
        _value2 = prop2.Value;

        var initialValueTask = Task.Run(async () => await _func(_value1, _value2));
        initialValueTask.Wait();
        SetNewValueSync(initialValueTask.Result);

        prop1.Subscribe(async (value1, token) =>
        {
            _value1 = value1;
            await UpdateAsync(token);
        });
        prop2.Subscribe(async (value2, token) =>
        {
            _value2 = value2;
            await UpdateAsync(token);
        });
    }

    private async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var result = await _func(_value1, _value2);
        await SetNewValueAsync(result, cancellationToken);
    }
}

public sealed class CombineLatestProperty<T1, T2, T3, TResult> : DeclarativePropertyBase<TResult>
{
    private readonly Func<T1?, T2?,T3?, Task<TResult>> _func;
    private T1? _value1;
    private T2? _value2;
    private T3? _value3;

    public CombineLatestProperty(
        IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        IDeclarativeProperty<T3> prop3,
        Func<T1?, T2?, T3?,Task<TResult>> func,
        Action<TResult>? setValueHook = null) : base(default!, setValueHook)
    {
        ArgumentNullException.ThrowIfNull(prop1);
        ArgumentNullException.ThrowIfNull(prop2);
        ArgumentNullException.ThrowIfNull(prop3);

        _func = func;

        _value1 = prop1.Value;
        _value2 = prop2.Value;
        _value3 = prop3.Value;

        var initialValueTask = Task.Run(async () => await _func(_value1, _value2, _value3));
        initialValueTask.Wait();
        SetNewValueSync(initialValueTask.Result);

        prop1.Subscribe(async (value1, token) =>
        {
            _value1 = value1;
            await UpdateAsync(token);
        });
        prop2.Subscribe(async (value2, token) =>
        {
            _value2 = value2;
            await UpdateAsync(token);
        });
        prop3.Subscribe(async (value3, token) =>
        {
            _value3 = value3;
            await UpdateAsync(token);
        });
    }

    private async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var result = await _func(_value1, _value2, _value3);
        await SetNewValueAsync(result, cancellationToken);
    }
}