﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TerminalUI;

public class EventLoop(IApplicationContext applicationContext,
        ILogger<EventLoop> logger)
    : IEventLoop
{
    private readonly ILogger<EventLoop> _logger = logger;
    private readonly List<Action> _initializers = new();
    private readonly List<Action> _permanentQueue = new();
    private readonly List<Action> _finalizers = new();

    public int ThreadId { get; set; } = -1;

    public void AddToPermanentQueue(Action action) => _permanentQueue.Add(action);
    public void AddInitializer(Action action) => _initializers.Add(action);
    public void AddFinalizer(Action action) => _finalizers.Add(action);

    public void Run()
    {
        applicationContext.IsRunning = true;
        ThreadId = Thread.CurrentThread.ManagedThreadId;
        foreach (var initializer in _initializers)
        {
            initializer();
        }
        while (applicationContext.IsRunning)
        {
            ProcessQueues();
            Thread.Sleep(10);
        }
        foreach (var finalizer in _finalizers)
        {
            finalizer();
        }
        ThreadId = -1;
    }

    private void ProcessQueues()
    {
        foreach (var action in _permanentQueue)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                _logger.LogError(e, "Error while processing action in permanent queue");
            }
        }
    }
}