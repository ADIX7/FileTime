﻿namespace DeclarativeProperty;

public class MergeProperty<T> : DeclarativePropertyBase<T>
{
    public MergeProperty(params IDeclarativeProperty<T>[] props) : base(default!)
    {
        ArgumentNullException.ThrowIfNull(props);

        foreach (var prop in props)
        {
            prop.Subscribe(UpdateAsync);
        }
    }

    private Task UpdateAsync(T newValue, CancellationToken token)
        => SetNewValueAsync(newValue, token);
}