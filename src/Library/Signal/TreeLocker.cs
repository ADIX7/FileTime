namespace Signal;

internal sealed class TreeLocker
{
    private bool Equals(TreeLocker other) => _mainSemaphore.Equals(other._mainSemaphore);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is TreeLocker other && Equals(other);

    public override int GetHashCode() => _mainSemaphore.GetHashCode();

    public static bool operator ==(TreeLocker? left, TreeLocker? right) => Equals(left, right);
    public static bool operator !=(TreeLocker? left, TreeLocker? right) => !Equals(left, right);

    private SemaphoreSlim _lastLockedMainSemaphore;
    private SemaphoreSlim _mainSemaphore = new(1, 1);
    private readonly SemaphoreSlim _semaphoreSemaphore = new(1, 1);

    public void Lock()
    {
        _semaphoreSemaphore.Wait();
        try
        {
            _lastLockedMainSemaphore = _mainSemaphore;
            _lastLockedMainSemaphore.Wait();
        }
        finally
        {
            _semaphoreSemaphore.Release();
        }
    }

    public Task LockAsync()
    {
        _semaphoreSemaphore.Wait();
        try
        {
            _lastLockedMainSemaphore = _mainSemaphore;
            return _lastLockedMainSemaphore.WaitAsync();
        }
        finally
        {
            _semaphoreSemaphore.Release();
        }
    }

    public void Release()
    {
        try
        {
            _semaphoreSemaphore.Wait();
            _lastLockedMainSemaphore.Release();
        }
        finally
        {
            _semaphoreSemaphore.Release();
        }
    }

    internal void UseInstead(TreeLocker newBaseLocker)
    {
        try
        {
            _semaphoreSemaphore.Wait();
            _mainSemaphore = newBaseLocker._mainSemaphore;
        }
        finally
        {
            _semaphoreSemaphore.Release();
        }
    }
}