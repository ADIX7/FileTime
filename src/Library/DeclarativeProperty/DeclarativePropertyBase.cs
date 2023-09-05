using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeclarativeProperty;

public abstract class DeclarativePropertyBase<T> : IDeclarativeProperty<T>
{
    private readonly List<Func<T?, CancellationToken, Task>> _subscribers = new();
    private readonly Action<T?>? _setValueHook;
    private readonly List<IDisposable> _disposables = new();
    private readonly List<Func<IDeclarativeProperty<T>, T, IDisposable?>> _subscribeTriggers = new();
    private readonly List<Action<IDeclarativeProperty<T>, T>> _unsubscribeTriggers = new();
    private readonly List<IDisposable> _triggerDisposables = new();
    private readonly object _triggerLock = new();
    private readonly object _subscriberLock = new();

    private T? _value;

    public T? Value
    {
        get => _value;
        set => _setValueHook?.Invoke(value);
    }

    protected DeclarativePropertyBase(Action<T?>? setValueHook = null)
    {
        _setValueHook = setValueHook;
    }

    protected DeclarativePropertyBase(T? initialValue, Action<T?>? setValueHook = null)
    {
        _setValueHook = setValueHook;
        _value = initialValue;
    }


    protected async Task NotifySubscribersAsync(T? newValue, CancellationToken cancellationToken = default)
    {
        List<Func<T?, CancellationToken, Task>> subscribers;
        lock (_subscriberLock)
        {
            subscribers = _subscribers.ToList();
        }

        foreach (var handler in subscribers)
        {
            await handler(newValue, cancellationToken);
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public IDisposable Subscribe(Func<T?, CancellationToken, Task> onChange)
    {
        lock (_subscriberLock)
        {
            _subscribers.Add(onChange);
        }

        onChange(_value, default);

        return new Unsubscriber<T>(this, onChange);
    }

    public void Unsubscribe(Func<T, CancellationToken, Task> onChange)
    {
        lock (_subscriberLock)
        {
            _subscribers.Remove(onChange!);
        }
    }

    public IDeclarativeProperty<T> RegisterTrigger(
        Func<IDeclarativeProperty<T>, T, IDisposable?> triggerSubscribe,
        Action<IDeclarativeProperty<T>, T>? triggerUnsubscribe = null)
    {
        lock (_triggerLock)
        {
            if (Value != null)
            {
                var disposable = triggerSubscribe(this, Value);
                if (disposable != null) _triggerDisposables.Add(disposable);
            }

            _subscribeTriggers.Add(triggerSubscribe);
            if (triggerUnsubscribe != null) _unsubscribeTriggers.Add(triggerUnsubscribe);
            return this;
        }
    }

    IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        => Subscribe((v, _) =>
        {
            observer.OnNext(v!);
            return Task.CompletedTask;
        });

    protected async Task SetNewValueAsync(T? newValue, CancellationToken cancellationToken = default)
    {
        SetNewValueSync(newValue, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        await NotifySubscribersAsync(newValue, cancellationToken);
    }

    protected void SetNewValueSync(T? newValue, CancellationToken cancellationToken = default)
    {
        if (!(Value?.Equals(newValue) ?? false))
        {
            lock (_triggerLock)
            {
                if (_value != null)
                {
                    foreach (var unsubscribeTrigger in _unsubscribeTriggers)
                    {
                        unsubscribeTrigger(this, _value);
                    }

                    foreach (var triggerDisposable in _triggerDisposables)
                    {
                        triggerDisposable.Dispose();
                    }

                    _triggerDisposables.Clear();
                }

                _value = newValue;

                if (_value != null)
                {
                    foreach (var subscribeTrigger in _subscribeTriggers)
                    {
                        var disposable = subscribeTrigger(this, _value);
                        if (disposable != null) _triggerDisposables.Add(disposable);
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested) return;

            OnPropertyChanged(nameof(Value));
        }
    }

    public async Task ReFireAsync()
        => await SetNewValueAsync(Value);

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        lock (_subscriberLock)
        {
            _subscribers.Clear();
        }
    }

    protected void AddDisposable(IDisposable disposable) => _disposables.Add(disposable);
}