namespace DeclarativeProperty;

public abstract class TimingPropertyBase<T> : DeclarativePropertyBase<T>
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    protected Func<TimeSpan> Interval { get; }

    protected TimingPropertyBase(
        IDeclarativeProperty<T> from,
        Func<TimeSpan> interval,
        Action<T?>? setValueHook = null) : base(from.Value, setValueHook)
    {
        Interval = interval;
        AddDisposable(from.Subscribe(SetValueInternal));
    }

    private async Task SetValueInternal(T? next, CancellationToken cancellationToken = default)
        => await WithLockAsync(async () => await SetValue(next, cancellationToken), cancellationToken);


    protected void WithLock(Action action)
    {
        try
        {
            _semaphore.Wait();
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected TResult WithLock<TResult>(Func<TResult> func)
    {
        try
        {
            _semaphore.Wait();
            return func();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected async Task WithLockAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected abstract Task SetValue(T? next, CancellationToken cancellationToken = default);

    protected async Task FireIfNeededAsync(
        T? next,
        Action cleanup,
        CancellationToken timingCancellationToken = default,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(Interval(), timingCancellationToken);
        var shouldFire = WithLock(() =>
        {
            if (timingCancellationToken.IsCancellationRequested)
                return false;

            cleanup();
            return true;
        });

        if (!shouldFire) return;

        await FireAsync(next, cancellationToken);
    }

    protected async Task FireAsync(T? next, CancellationToken cancellationToken = default) 
        => await SetNewValueAsync(next, cancellationToken);
}