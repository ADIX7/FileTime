namespace Signal;

public class MapSignal<T, TResult> : SignalBase<TResult>
{
    private readonly Func<ValueTask<TResult>> _map;
    private TResult _result;
    public MapSignal(IReadOnlySignal<T> signal, Func<T, TResult> map) : base(signal)
    {
        _map = MapValueAsync;
        
        async ValueTask<TResult> MapValueAsync() => map(await signal.GetValueAsync());
    }
    public MapSignal(IReadOnlySignal<T> signal, Func<T, Task<TResult>> map) : base(signal)
    {
        _map = MapValueAsync;
        
        async ValueTask<TResult> MapValueAsync() => await map(await signal.GetValueAsync());
    }

    public override async ValueTask<TResult> GetValueAsync()
    {
        //TODO synchronization
        if (!IsDirty)
        {
            return _result;
        }
        IsDirty = false;
        _result = await _map();
        return _result;
    }
}