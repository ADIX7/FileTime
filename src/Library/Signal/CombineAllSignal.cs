namespace Signal;

public class CombineAllSignal<T, TResult> : SignalBase<TResult>
{
    private readonly IReadOnlyList<SignalBase<T>> _parentSignals;
    private readonly Func<ICollection<T>, ValueTask<TResult>> _combiner;
    private readonly T[] _lastParentResults;
    private readonly T[] _currentParentResults;
    private TResult? _lastResult;
    private bool _isInitialized;

    public CombineAllSignal(ICollection<SignalBase<T>> parentSignals, Func<ICollection<T>, TResult> combiner) :
        base(parentSignals)
    {
        _parentSignals = parentSignals.ToList();
        _combiner = c => new ValueTask<TResult>(combiner(c));

        _lastParentResults = new T[_parentSignals.Count];
        _currentParentResults = new T[_parentSignals.Count];
    }

    public CombineAllSignal(ICollection<SignalBase<T>> parentSignals, Func<ICollection<T>, ValueTask<TResult>> combiner) :
        base(parentSignals)
    {
        _parentSignals = parentSignals.ToList();
        _combiner = combiner;

        _lastParentResults = new T[_parentSignals.Count];
        _currentParentResults = new T[_parentSignals.Count];
    }

    protected override async ValueTask<TResult> GetValueInternalAsync()
    {
        if (!IsDirty)
        {
            return _lastResult!;
        }

        IsDirty = false;

        if (_isInitialized)
        {
            bool anyChanged = false;
            for (var i = 0; i < _parentSignals.Count; i++)
            {
                var parentSignal = _parentSignals[i];
                var newResult = await parentSignal.GetValueAsync();
                _currentParentResults[i] = newResult;

                if ((newResult == null && _lastParentResults[i] != null)
                    || (newResult != null && newResult.Equals(_lastParentResults[i])))
                {
                    anyChanged = true;
                }
            }

            if (!anyChanged)
            {
                return _lastResult!;
            }
        }

        _isInitialized = true;
        Array.Copy(_currentParentResults, _lastParentResults, _currentParentResults.Length);
        var result = await _combiner(_currentParentResults);
        _lastResult = result;
        return result;
    }
}