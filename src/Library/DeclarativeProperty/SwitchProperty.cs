namespace DeclarativeProperty;

public sealed class SwitchProperty<TItem> : DeclarativePropertyBase<TItem>
{
    private IDisposable? _innerSubscription;

    public SwitchProperty(IDeclarativeProperty<IDeclarativeProperty<TItem>?> from) : base(from.Value is null ? default! : from.Value.Value)
    {
        AddDisposable(from.Subscribe(HandleStreamChange));
        _innerSubscription = from.Value?.Subscribe(HandleInnerValueChange);
    }

    private async Task HandleStreamChange(IDeclarativeProperty<TItem>? next, CancellationToken token)
    {
        _innerSubscription?.Dispose();
        _innerSubscription = next?.Subscribe(HandleInnerValueChange);

        await SetNewValueAsync(next is null ? default! : next.Value, token);
    }

    private Task HandleInnerValueChange(TItem next, CancellationToken token)
        => SetNewValueAsync(next, token);
}