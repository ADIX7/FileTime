namespace DeclarativeProperty;

internal sealed class Unsubscriber<T> : IDisposable
{
    private readonly IDeclarativeProperty<T> _owner;
    private readonly Func<T?, CancellationToken, Task> _onChange;

    public Unsubscriber(IDeclarativeProperty<T> owner, Func<T?, CancellationToken, Task> onChange)
    {
        _owner = owner;
        _onChange = onChange;
    }
    public void Dispose() => _owner.Unsubscribe(_onChange);
}