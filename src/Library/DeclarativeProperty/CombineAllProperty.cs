namespace DeclarativeProperty;

public class CombineAllProperty<T, TResult> : DeclarativePropertyBase<TResult>
{
    private readonly List<IDeclarativeProperty<T>> _sources;
    private readonly Func<IEnumerable<T>, Task<TResult>> _combiner;

    public CombineAllProperty(
        IEnumerable<IDeclarativeProperty<T>> sources,
        Func<IEnumerable<T>, Task<TResult>> combiner,
        Action<TResult>? setValueHook = null) : base(default!, setValueHook)
    {
        var sourcesList = sources.ToList();
        _sources = sourcesList;
        _combiner = combiner;


        var result = Task.Run(async () => await _combiner(sourcesList.Select(p => p.Value)))
            .GetAwaiter()
            .GetResult();
        SetNewValueSync(result);

        foreach (var declarativeProperty in sourcesList)
        {
            AddDisposable(declarativeProperty.Subscribe(OnSourceChanged));
        }
    }

    private async Task OnSourceChanged(T arg1, CancellationToken arg2) => await Update();

    private async Task Update()
    {
        var values = _sources.Select(p => p.Value);
        var result = await _combiner(values);

        await SetNewValueAsync(result);
    }
}