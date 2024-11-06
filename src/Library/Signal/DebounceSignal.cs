namespace Signal;

public class DebounceSignal<T> : SignalBase<T>
{
    private readonly object _debounceTaskLock = new();
    private readonly SignalBase<T> _parentSignal;
    private bool _isDebounceRunning;
    private DateTime _debounceStartedAt;
    private readonly Func<TimeSpan> _interval;
    private T? _lastParentValue;

    public bool ResetTimer { get; init; }
    public TimeSpan WaitInterval { get; init; } = TimeSpan.FromMilliseconds(10);
    
    public override bool IsDirty
    {
        get => base.IsDirty;
        protected set
        {
            if(!value)
            {
                base.IsDirty = value;
                return;
            }

            lock (_debounceTaskLock)
            {
                if (_isDebounceRunning)
                {
                    if (ResetTimer)
                    {
                        _debounceStartedAt = DateTime.Now;
                    }

                    return;
                }
                _isDebounceRunning = true;
                _debounceStartedAt = DateTime.Now;
                Task.Run(StartDebouncing);
            }
        }
    }

    async Task StartDebouncing()
    {
        while (DateTime.Now - _debounceStartedAt < _interval())
        {
            await Task.Delay(WaitInterval);
        }
        
        base.IsDirty = true;
        lock (_debounceTaskLock)
        {
            _isDebounceRunning = false;
        }
    }

    public DebounceSignal(SignalBase<T> parentSignal, Func<TimeSpan> interval) : base(parentSignal)
    {
        _parentSignal = parentSignal;
        _interval = interval;
    }
    
    protected override async ValueTask<T> GetValueInternalAsync()
    {
        if (!IsDirty)
        {
            return _lastParentValue!;
        }

        IsDirty = false;
        var baseValue = await _parentSignal.GetValueAsync();
        if (
            (_lastParentValue == null && baseValue == null) ||
            (baseValue != null && baseValue.Equals(_lastParentValue)))
        {
            return _lastParentValue!;
        }

        _lastParentValue = baseValue;
        return _lastParentValue;
    }
}