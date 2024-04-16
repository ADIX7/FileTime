namespace Signal;

public abstract class SignalBase<T> : IReadOnlySignal<T>
{
    private readonly List<SignalBase<T>> _dependentSignals = [];
    public bool IsDirty { get; protected set; } = true;
    public event Action<bool>? IsDirtyChanged;

    public SignalBase()
    {
        
    }

    public SignalBase(IReadOnlySignal baseSignal)
    {
        HandleDependentSignal(baseSignal);
    }

    public SignalBase(IEnumerable<IReadOnlySignal> baseSignal)
    {
        foreach (var signal in baseSignal)
        {
            HandleDependentSignal(signal);
        }
    }
    
    private void HandleDependentSignal(IReadOnlySignal baseSignal)
    {
        baseSignal.IsDirtyChanged += isDirty =>
        {
            if (isDirty)
            {
                SetDirty();
            }
        };
    }

    public void SetDirty()
    {
        IsDirty = true;
        for (var i = 0; i < _dependentSignals.Count; i++)
        {
            _dependentSignals[i].SetDirty();
        }

        IsDirtyChanged?.Invoke(IsDirty);
    }

    public abstract ValueTask<T> GetValueAsync();
}