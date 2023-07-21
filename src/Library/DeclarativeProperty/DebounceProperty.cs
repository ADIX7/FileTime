namespace DeclarativeProperty;

public sealed class DebounceProperty<T> : TimingPropertyBase<T>
{
    private CancellationTokenSource? _debounceCts;
    private bool _isActive;
    private DateTime _startTime;
    public bool ResetTimer { get; init; }
    public TimeSpan WaitInterval { get; init; } = TimeSpan.FromMilliseconds(1);

    public DebounceProperty(
        IDeclarativeProperty<T> from,
        TimeSpan interval,
        Action<T?>? setValueHook = null) : base(from, interval, setValueHook)
    {
    }

    protected override Task SetValue(T? next, CancellationToken cancellationToken = default)
    {
        _debounceCts?.Cancel();
        var newTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _debounceCts = newTokenSource;

        var newToken = newTokenSource.Token;
        
        if (!_isActive || ResetTimer)
        {
            _isActive = true;
            _startTime = DateTime.Now;
        }

        Task.Run(async () =>
        {
            try
            {
                while (DateTime.Now - _startTime < Interval)
                {
                    await Task.Delay(WaitInterval, newToken);
                }

                WithLock(() => { _isActive = false; });

                await FireAsync(next, cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
            }
        });

        return Task.CompletedTask;
    }
}