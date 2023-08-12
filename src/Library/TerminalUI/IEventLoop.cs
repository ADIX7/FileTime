namespace TerminalUI;

public interface IEventLoop
{
    void Run();
    void AddToPermanentQueue(Action action);
}