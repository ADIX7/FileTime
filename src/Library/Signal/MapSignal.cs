namespace Signal;

public sealed class MapSignal<T, TResult> : SignalBase<TResult>
{
    private readonly Func<T, ValueTask<TResult>> _map;
    private readonly SignalBase<T> _parentSignal;
    private T? _lastParentValue;
    private TResult? _lastResult;

    private MapSignal(SignalBase<T> signal) : base(signal)
    {
        _parentSignal = signal;
    }

    public MapSignal(SignalBase<T> signal, Func<T, TResult> map) : this(signal)
    {
        _map = MapValueAsync;

        ValueTask<TResult> MapValueAsync(T val) => new(map(val));
    }

    public MapSignal(SignalBase<T> signal, Func<T, Task<TResult>> map) : this(signal)
    {
        _map = MapValueAsync;

        async ValueTask<TResult> MapValueAsync(T val) => await map(val);
    }

    public MapSignal(SignalBase<T> signal, Func<T, ValueTask<TResult>> map) : this(signal)
    {
        _map = MapValueAsync;

        async ValueTask<TResult> MapValueAsync(T val) => await map(val);
    }

    protected override async ValueTask<TResult> GetValueInternalAsync()
    {
        if (!IsDirty)
        {
            return _lastResult!;
        }

        IsDirty = false;
        var baseValue = await _parentSignal.GetValueAsync();
        if (
            (_lastParentValue == null && baseValue == null) ||
            (baseValue != null && baseValue.Equals(_lastParentValue)))
        {
            return _lastResult!;
        }

        _lastParentValue = baseValue;
        _lastResult = await _map(baseValue);
        return _lastResult;
    }
}