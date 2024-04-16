namespace Signal;

public interface IReadOnlySignal
{
    bool IsDirty { get; }
    event Action<bool> IsDirtyChanged;
    internal void SetDirty();
}
public interface IReadOnlySignal<T> : IReadOnlySignal
{
    ValueTask<T> GetValueAsync();
}