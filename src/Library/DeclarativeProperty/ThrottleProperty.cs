﻿namespace DeclarativeProperty;

public class ThrottleProperty<T> : DeclarativePropertyBase<T>
{
    private readonly object _lock = new();
    private DateTime _lastFired = DateTime.MinValue;
    private readonly Func<TimeSpan> _interval;

    public ThrottleProperty(
        IDeclarativeProperty<T> from,
        Func<TimeSpan> interval,
        Action<T>? setValueHook = null) : base(from.Value, setValueHook)
    {
        _interval = interval;
        AddDisposable(from.Subscribe(SetValue));
    }

    private Task SetValue(T next, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (DateTime.Now - _lastFired < _interval())
            {
                return Task.CompletedTask;
            }

            _lastFired = DateTime.Now;
        }

        return SetNewValueAsync(
            next,
            cancellationToken
        );
    }
}