namespace TerminalUI;

public interface IEventLoop
{
    void Run();
    void AddToPermanentQueue(Action action);
    void AddInitializer(Action action);
    int ThreadId { get; set; }
    void AddFinalizer(Action action);
}