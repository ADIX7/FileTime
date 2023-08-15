using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TerminalUI;

public class EventLoop : IEventLoop
{
    private readonly IApplicationContext _applicationContext;
    private readonly ILogger<EventLoop> _logger;
    private readonly List<Action> _permanentQueue = new();

    public EventLoop(
        IApplicationContext applicationContext,
        ILogger<EventLoop> logger)
    {
        _applicationContext = applicationContext;
        _logger = logger;
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
            /*try
            {*/
                action();
            /*}
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                _logger.LogError(e, "Error while processing action in permanent queue");
            }*/
        }
    }
}