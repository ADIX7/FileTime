namespace System;

public static class DeferTools
{
    public static IDisposable Defer(Action action) => new DeferDisposable(action);
}

internal readonly struct DeferDisposable : IDisposable
{
    readonly Action _action;
    public DeferDisposable(Action action) => _action = action;
    public void Dispose() => _action();
}
