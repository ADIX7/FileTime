using System.ComponentModel;

namespace DeclarativeProperty;

public interface IDeclarativeProperty<T> : INotifyPropertyChanged, IDisposable, IObservable<T>
{
    T Value { get; set; }
    IDisposable Subscribe(Func<T, CancellationToken, Task> onChange);
    void Unsubscribe(Func<T, CancellationToken, Task> onChange);

    IDeclarativeProperty<T> RegisterTrigger(
        Func<IDeclarativeProperty<T>, T, IDisposable?> triggerSubscribe,
        Action<IDeclarativeProperty<T>, T>? triggerUnsubscribe = null
    );

    Task ReFireAsync();
}