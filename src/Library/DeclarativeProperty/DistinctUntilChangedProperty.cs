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

    private Task Handle(T next, CancellationToken cancellationToken = default)
    {
        if (_comparer is { } comparer)
        {
            if (comparer(Value, next))
            {
                return Task.CompletedTask;
            }
        }
        else if (
            (next is null && Value is null && !_firstFire)
            || (Value?.Equals(next) ?? false)
        )
        {
            return Task.CompletedTask;
        }

        _firstFire = false;
        return SetNewValueAsync(next, cancellationToken);
    }
}