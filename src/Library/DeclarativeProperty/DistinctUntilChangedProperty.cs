namespace DeclarativeProperty;

public sealed class DistinctUntilChangedProperty<T> : DeclarativePropertyBase<T>
{
    private readonly Func<T?, T?, bool>? _comparer;
    private bool _firstFire = true;

    public DistinctUntilChangedProperty(IDeclarativeProperty<T> from, Func<T?, T?, bool>? comparer = null) : base(from.Value)
    {
        _comparer = comparer;
        AddDisposable(from.Subscribe(Handle));
    }

    async Task Handle(T next, CancellationToken cancellationToken = default)
    {
        if (_comparer is { } comparer)
        {
            if (comparer(Value, next))
            {
                return;
            }
        }
        else if (
            (next is null && Value is null && !_firstFire)
            || (Value?.Equals(next) ?? false)
        )
        {
            return;
        }

        _firstFire = false;
        await SetNewValueAsync(next, cancellationToken);
    }
}