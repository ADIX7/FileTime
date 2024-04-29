namespace Signal;

public sealed class Signal<T> : SignalBase<T>, ISignal<T>
{
    private T _value;

    public Signal(T value)
    {
        _value = value;
    }

    public void SetValue(T value)
    {
        TreeLock.Lock();
        try
        {
            _value = value;
            IsDirty = true;
        }
        finally
        {
            TreeLock.Release();
        }
    }

    public async Task SetValueAsync(T value)
    {
        await TreeLock.LockAsync();
        try
        {
            _value = value;
            IsDirty = true;
        }
        finally
        {
            TreeLock.Release();
        }
    }

    protected override ValueTask<T> GetValueInternalAsync()
    {
        IsDirty = false;
        return new ValueTask<T>(_value);
    }
}
