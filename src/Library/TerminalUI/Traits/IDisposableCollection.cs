namespace TerminalUI.Traits;

public interface IDisposableCollection : IDisposable
{
    void AddDisposable(IDisposable disposable);
    void RemoveDisposable(IDisposable disposable);
}