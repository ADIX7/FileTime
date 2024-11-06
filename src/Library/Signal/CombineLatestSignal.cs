namespace Signal;

public sealed class CombineLatestSignal<T1, T2, TResult> : SignalBase<TResult>
{
    private readonly Func<ValueTask<TResult>> _combine;
    private TResult _result;
    
    public CombineLatestSignal(SignalBase<T1> signal1, SignalBase<T2> signal2, Func<T1, T2, TResult> combine) 
        : base(new SignalBase[] { signal1, signal2 })
    {
        _combine = CombineAsync;
        
        async ValueTask<TResult> CombineAsync()
        {
            var val1 = await signal1.GetValueAsync();
            var val2 = await signal2.GetValueAsync();
            return combine(val1, val2);
        }
    }
    
    public CombineLatestSignal(SignalBase<T1> signal1, SignalBase<T2> signal2, Func<T1, T2, Task<TResult>> combine) 
        : base(new SignalBase[] { signal1, signal2 })
    {
        _combine = CombineAsync;
        
        async ValueTask<TResult> CombineAsync()
        {
            var val1 = await signal1.GetValueAsync();
            var val2 = await signal2.GetValueAsync();
            return await combine(val1, val2);
        }
    }

    protected override async ValueTask<TResult> GetValueInternalAsync()
    {
        if (!IsDirty)
        {
            return _result;
        }
        
        // TODO caching
        IsDirty = false;
        _result = await _combine();
        return _result;
    }
}