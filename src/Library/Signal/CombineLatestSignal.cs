namespace Signal;

public class CombineLatestSignal<T1, T2, TResult> : SignalBase<TResult>
{
    private readonly Func<ValueTask<TResult>> _combine;
    private TResult _result;
    
    public CombineLatestSignal(IReadOnlySignal<T1> signal1, IReadOnlySignal<T2> signal2, Func<T1, T2, TResult> combine) 
        : base(new IReadOnlySignal[] { signal1, signal2 })
    {
        _combine = CombineAsync;
        
        async ValueTask<TResult> CombineAsync() => combine(await signal1.GetValueAsync(), await signal2.GetValueAsync());
        
    }
    
    public CombineLatestSignal(IReadOnlySignal<T1> signal1, IReadOnlySignal<T2> signal2, Func<T1, T2, Task<TResult>> combine) 
        : base(new IReadOnlySignal[] { signal1, signal2 })
    {
        _combine = CombineAsync;
        
        async ValueTask<TResult> CombineAsync() => await combine(await signal1.GetValueAsync(), await signal2.GetValueAsync());
    }

    public override async ValueTask<TResult> GetValueAsync()
    {
        //TODO synchronization
        if (!IsDirty)
        {
            return _result;
        }
        IsDirty = false;
        _result = await _combine();
        return _result;
    }
}