namespace Signal;

public abstract class SignalBase : IReadOnlySignal
{
    private bool _isDirty = true;

    public event Action<bool>? IsDirtyChanged;

    public virtual bool IsDirty
    {
        get => _isDirty;
        protected set
        {
            if (_isDirty == value)
            {
                return;
            }

            _isDirty = value;
            IsDirtyChanged?.Invoke(value);
        }
    }

    public event Action<SignalBase> Disposed;
    public bool IsDisposed { get; private set; }

    internal TreeLocker TreeLock { get; }

    private protected SignalBase(TreeLocker treeTreeLock)
    {
        TreeLock = treeTreeLock;
    }

    public virtual void Dispose()
    {
        // TODO: disposing pattern
        IsDisposed = true;
        Disposed?.Invoke(this);
    }
}

public abstract class SignalBase<T> : SignalBase, IReadOnlySignal<T>
{
    internal static AsyncLocal<TreeLocker> CurrentTreeLocker { get; } = new();

    private protected SignalBase() : base(new TreeLocker())
    {
    }

    protected SignalBase(SignalBase parentSignal) : base(parentSignal.TreeLock)
    {
        SubscribeToParentSignalChanges(parentSignal);
    }

    protected SignalBase(ICollection<SignalBase> parentSignals) : base(CreateMultiParentTreeLock(parentSignals))
    {
        ArgumentOutOfRangeException.ThrowIfZero(parentSignals.Count);

        foreach (var parentSignal in parentSignals)
        {
            SubscribeToParentSignalChanges(parentSignal);
        }
    }

    protected SignalBase(IEnumerable<SignalBase> parentSignals) : base(CreateMultiParentTreeLock(parentSignals))
    {
        if (!parentSignals.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(parentSignals));
        }

        foreach (var parentSignal in parentSignals)
        {
            SubscribeToParentSignalChanges(parentSignal);
        }
    }

    private static TreeLocker CreateMultiParentTreeLock(IEnumerable<SignalBase> parentSignals)
    {
        var firstLock = parentSignals.First().TreeLock;
        foreach (var parentSignal in parentSignals.Skip(1))
        {
            parentSignal.TreeLock.UseInstead(firstLock);
        }

        return firstLock;
    }

    private void SubscribeToParentSignalChanges(SignalBase parentSignal)
    {
        // Note: Do not forget to unsubscribe from the parent signal when this signal is disposed.
        parentSignal.IsDirtyChanged += HandleParentIsDirtyChanged;
        parentSignal.Disposed += UnsubscribeFromParentSignalChangesAndDispose;
    }

    private void HandleParentIsDirtyChanged(bool isDirty)
    {
        if (isDirty)
        {
            IsDirty = true;
        }
    }

    private void UnsubscribeFromParentSignalChangesAndDispose(SignalBase parentSignal)
    {
        parentSignal.IsDirtyChanged -= HandleParentIsDirtyChanged;
        parentSignal.Disposed -= UnsubscribeFromParentSignalChangesAndDispose;

        Dispose();
    }

    protected abstract ValueTask<T> GetValueInternalAsync();

    public async ValueTask<T> GetValueAsync()
    {
        var shouldReleaseLock = false;
        if (CurrentTreeLocker.Value != TreeLock)
        {
            await TreeLock.LockAsync();
            shouldReleaseLock = true;
            CurrentTreeLocker.Value = TreeLock;
        }

        try
        {
            return await GetValueInternalAsync();
        }
        finally
        {
            if (shouldReleaseLock)
            {
                CurrentTreeLocker.Value = null;
                TreeLock.Release();
            }
        }
    }
}