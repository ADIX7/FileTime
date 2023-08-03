namespace DeclarativeProperty;

public class MergeProperty<T> : DeclarativePropertyBase<T>
{
    public MergeProperty(params IDeclarativeProperty<T>[] props)
    {
        ArgumentNullException.ThrowIfNull(props);

        foreach (var prop in props)
        {
            prop.Subscribe(UpdateAsync);
        }
    }

    private async Task UpdateAsync(T? newValue, CancellationToken token)
        => await SetNewValueAsync(newValue, token);
}