namespace DeclarativeProperty;

public sealed class MapProperty<TFrom, TTo> : DeclarativePropertyBase<TTo>
{
    private readonly Func<TFrom, CancellationToken, Task<TTo>> _mapper;

    public MapProperty(
        Func<TFrom, CancellationToken, Task<TTo>> mapper, 
        IDeclarativeProperty<TFrom> from,
        Action<TTo>? setValueHook = null) : base(default!, setValueHook)
    {
        _mapper = mapper;
        
        var initialValueTask = Task.Run(async () => await _mapper(from.Value, CancellationToken.None));
        initialValueTask.Wait();
        SetNewValueSync(initialValueTask.Result);

        AddDisposable(from.Subscribe(SetValue));
    }

    private async Task SetValue(TFrom next, CancellationToken cancellationToken = default)
    {
        var newValue = await _mapper(next!, cancellationToken);
        await SetNewValueAsync(newValue, cancellationToken);
    }
    
    public static async Task<MapProperty<TFrom, TTo>> CreateAsync(
        Func<TFrom, CancellationToken, Task<TTo>> mapper, 
        IDeclarativeProperty<TFrom> from,
        Action<TTo>? setValueHook = null)
    {
        var prop = new MapProperty<TFrom, TTo>(mapper, from, setValueHook);
        await prop.SetValue(from.Value);

        return prop;
    }
}