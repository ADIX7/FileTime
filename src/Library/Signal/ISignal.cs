namespace Signal;

public interface ISignal<T> : IReadOnlySignal<T>
{
    void SetValue(T value);
}