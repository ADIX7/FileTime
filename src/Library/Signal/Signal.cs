namespace Signal;

public class Signal<T> : SignalBase<T>, ISignal<T>
{
    private T _value;

    public Signal(T value)
    {
        _value = value;
    }

    public void SetValue(T value)
    {
        _value = value;
        SetDirty();
    }
    
    public override ValueTask<T> GetValueAsync()
    {
        IsDirty = false;
        return new ValueTask<T>(_value);
    }
}