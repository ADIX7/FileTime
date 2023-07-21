namespace DeclarativeProperty;

public sealed class CombineLatestProperty<T1, T2, TResult> : DeclarativePropertyBase<TResult>
{
    private readonly Func<T1?, T2?, Task<TResult>> _func;
    private T1? _value1;
    private T2? _value2;

    public CombineLatestProperty(IDeclarativeProperty<T1> prop1, IDeclarativeProperty<T2> prop2, Func<T1?, T2?, Task<TResult>> func)
    {
        ArgumentNullException.ThrowIfNull(prop1);
        ArgumentNullException.ThrowIfNull(prop2);
        
        _func = func;

        _value1 = prop1.Value is null ? default : prop1.Value;
        _value2 = prop2.Value is null ? default : prop2.Value;
        
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