namespace DeclarativeProperty;

public sealed class FilterProperty<T> : DeclarativePropertyBase<T>
{
    private readonly Func<T?, Task<bool>> _filter;

    public FilterProperty(
        Func<T?, Task<bool>> filter,
        IDeclarativeProperty<T> from,
        Action<T?>? setValueHook = null) : base(setValueHook)
    {
        _filter = filter;
        AddDisposable(from.Subscribe(SetValue));
    }

    private async Task SetValue(T? next, CancellationToken cancellationToken = default)
    {
        if (await _filter(next))
        {
            await SetNewValueAsync(next, cancellationToken);
        }
    }
    
    public static async Task<FilterProperty<T>> CreateAsync(
        Func<T?, Task<bool>> filter, 
        IDeclarativeProperty<T> from,
        Action<T?>? setValueHook = null)
    {
        var prop = new FilterProperty<T>(filter, from, setValueHook);
        await prop.SetValue(from.Value);

        return prop;
    }
}