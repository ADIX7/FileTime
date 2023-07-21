namespace DeclarativeProperty;

public class ThrottleProperty<T> : TimingPropertyBase<T>
{
    private CancellationTokenSource? _debounceCts;
    private DateTime _lastFired;

    public ThrottleProperty(
        IDeclarativeProperty<T> from,
        TimeSpan interval,
        Action<T?>? setValueHook = null) : base(from, interval, setValueHook)
    {
    }

    protected override Task SetValue(T? next, CancellationToken cancellationToken = default)
    {
        _debounceCts?.Cancel();
        if (DateTime.Now - _lastFired > Interval)
        {
            _lastFired = DateTime.Now;
            // Note: Recursive chains can happen. Awaiting this can cause a deadlock.
            Task.Run(async () => await FireAsync(next, cancellationToken));
        }
        else
        {
            _debounceCts = new();
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Interval, _debounceCts.Token);
                    await FireIfNeededAsync(
                        next,
                        () => { _lastFired = DateTime.Now; },
                        _debounceCts.Token, cancellationToken
                    );
                    /*var shouldFire = WithLock(() =>
                    {
                        if (_debounceCts.Token.IsCancellationRequested)
                            return false;

                        _lastFired = DateTime.Now;
                        return true;
                    });

                    if (!shouldFire) return;

                    await Fire(next, cancellationToken);*/
                }
                catch (TaskCanceledException ex)
                {
                }
            });
        }
        
        return Task.CompletedTask;
    }
}