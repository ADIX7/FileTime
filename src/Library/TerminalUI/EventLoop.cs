namespace TerminalUI;

public class EventLoop : IEventLoop
{
    private readonly IApplicationContext _applicationContext;
    private readonly List<Action> _permanentQueue = new();

    public EventLoop(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public void AddToPermanentQueue(Action action) => _permanentQueue.Add(action);

    public void Run()
    {
        _applicationContext.IsRunning = true;
        while (_applicationContext.IsRunning)
        {
            ProcessQueues();
            Thread.Sleep(10);
        }
    }

    private void ProcessQueues()
    {
        foreach (var action in _permanentQueue)
        {
            action();
        }
    }
}