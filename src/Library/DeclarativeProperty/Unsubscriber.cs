namespace DeclarativeProperty;

internal sealed class Unsubscriber<T>(IDeclarativeProperty<T> owner, Func<T, CancellationToken, Task> onChange)
    : IDisposable
{
    public void Dispose() => owner.Unsubscribe(onChange);
}