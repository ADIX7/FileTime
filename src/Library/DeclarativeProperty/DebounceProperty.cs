namespace DeclarativeProperty;

public sealed class DebounceProperty<T> : DeclarativePropertyBase<T>
{
    private readonly object _lock = new();
    private readonly Func<TimeSpan> _interval;
    private DateTime _startTime = DateTime.MinValue;
    private T? _nextValue;
    private CancellationToken _nextCancellationToken;
    private bool _isThrottleTaskRunning;
    public bool ResetTimer { get; init; }
    public TimeSpan WaitInterval { get; init; } = TimeSpan.FromMilliseconds(10);

    public DebounceProperty(
        IDeclarativeProperty<T> from,
        Func<TimeSpan> interval,
        Action<T?>? setValueHook = null) : base(from.Value, setValueHook)
    {
        _interval = interval;
        AddDisposable(from.Subscribe(SetValue));
    }

    private Task SetValue(T? next, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _nextValue = next;
            _nextCancellationToken = cancellationToken;
            
            if (_isThrottleTaskRunning)
            {
                if (ResetTimer)
                {
                    _startTime = DateTime.Now;
                }
                return Task.CompletedTask;
            }

            _startTime = DateTime.Now;
            _isThrottleTaskRunning = true;
            Task.Run(async () => await StartDebounceTask());
        }

        return Task.CompletedTask;
    }

    private async Task StartDebounceTask()
    {
        while (DateTime.Now - _startTime < _interval())
        {
            await Task.Delay(WaitInterval);
        }

        T? next;
        CancellationToken cancellationToken;
        lock (_lock)
        {
            _isThrottleTaskRunning = false;
            next = _nextValue;
            cancellationToken = _nextCancellationToken;
        }

        await SetNewValueAsync(
            next,
            cancellationToken
        );
    }
}