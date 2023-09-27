namespace DeclarativeProperty;

public sealed class CombineProperty<TFrom, TTo> : DeclarativePropertyBase<TTo>
{
    private readonly Func<IReadOnlyList<TFrom?>, Task<TTo>> _combiner;
    private readonly List<IDeclarativeProperty<TFrom>> _sourceProperties = new();

    public CombineProperty(Func<IReadOnlyList<TFrom?>, Task<TTo>> combiner) : base(default!)
    {
        _combiner = combiner;
    }

    public async Task AddSourceAsync(IDeclarativeProperty<TFrom> source)
    {
        if (_sourceProperties.Contains(source)) return;
        _sourceProperties.Add(source);
        source.Subscribe(OnSourceChanged);

        await Update();
    }

    public async Task RemoveSource(IDeclarativeProperty<TFrom> source)
    {
        _sourceProperties.Remove(source);
        source.Unsubscribe(OnSourceChanged);

        await Update();
    }

    private async Task OnSourceChanged(TFrom? _, CancellationToken cancellationToken = default)
        => await Update(cancellationToken);

    private async Task Update(CancellationToken cancellationToken = default)
    {
        var result = await _combiner(
            _sourceProperties
                .Select(p => p.Value)
                .ToList()
                .AsReadOnly()
        );

        await SetNewValueAsync(result, cancellationToken);
    }
}