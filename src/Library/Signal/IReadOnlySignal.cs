namespace Signal;

public interface IReadOnlySignal : IDisposable
{
    bool IsDirty { get; }
    event Action<bool> IsDirtyChanged;
}
public interface IReadOnlySignal<T> : IReadOnlySignal
{
    ValueTask<T> GetValueAsync();
}