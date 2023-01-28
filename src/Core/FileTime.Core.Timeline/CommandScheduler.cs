using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public class CommandScheduler : ICommandScheduler
{
    private readonly SourceList<ParallelCommands> _commandsToRun = new();
    private readonly List<ICommandExecutor> _commandExecutors = new();
    private readonly Subject<FullName> _containerToRefresh = new();

    private readonly object _guard = new();
    private bool _enableRunning = true;
    private bool _resourceIsInUse;

    public IObservable<FullName> ContainerToRefresh { get; }

    public bool EnableRunning
    {
        get
        {
            var result = true;
            RunWithLock(() => result = _enableRunning);
            return result;
        }

        set { RunWithLock(() => _enableRunning = value); }
    }

    public CommandScheduler(ILocalCommandExecutor localExecutor)
    {
        ContainerToRefresh = _containerToRefresh.AsObservable();

        localExecutor.CommandFinished += LocalExecutorOnCommandFinished;
        _commandExecutors.Add(localExecutor);
    }

    public async Task AddCommand(ICommand command, int? batchId = null, bool toNewBatch = false)
    {
        await RunWithLockAsync(async () =>
        {
            ParallelCommands batchToAdd;

            if (_commandsToRun.Count == 0)
            {
                batchToAdd = new ParallelCommands(PointInTime.CreateEmpty());
                _commandsToRun.Add(batchToAdd);
            }
            else if (toNewBatch)
            {
                batchToAdd = new ParallelCommands(_commandsToRun.Items.Last().Result);
                _commandsToRun.Add(batchToAdd);
            }
            else if (batchId != null && _commandsToRun.Items.First(b => b.Id == batchId) is { } parallelCommands)
            {
                batchToAdd = parallelCommands;
            }
            else
            {
                batchToAdd = _commandsToRun.Items.First();
            }

            await batchToAdd.AddCommand(command);

            await RefreshCommands();

            ExecuteCommands();

            /*if (_commandRunners.Count == 0)
            {
                StartCommandRunner();
            }

            await UpdateReadOnlyCommands();*/
        });
    }

    public void RefreshContainer(FullName container) => _containerToRefresh.OnNext(container);

    private void ExecuteCommands()
    {
        if (!_enableRunning) return;

        var parallelCommandsToExecute = _commandsToRun.Items.FirstOrDefault();
        if (parallelCommandsToExecute is null ||
            parallelCommandsToExecute.Commands.All(c => c.ExecutionState != ExecutionState.Waiting)) return;

        var commandsToExecute = parallelCommandsToExecute.Commands;

        foreach (var commandToExecute in commandsToExecute)
        {
            if (commandToExecute.ExecutionState != ExecutionState.Waiting) continue;

            var commandExecutor = GetCommandExecutor();

            commandExecutor.ExecuteCommand(commandToExecute.Command);

            commandToExecute.ExecutionState = ExecutionState.Running;
        }
    }

    private ICommandExecutor GetCommandExecutor()
    {
        //TODO
        return _commandExecutors[0];
    }

    private void LocalExecutorOnCommandFinished(object? sender, ICommand command)
    {
        var parallelCommandsToExecute = _commandsToRun.Items.FirstOrDefault();
        if (parallelCommandsToExecute is null) return;

        var state = parallelCommandsToExecute.Commands.FirstOrDefault(c => c.Command == command);
        if (state is null) return;

        state.ExecutionState = ExecutionState.Finished;
    }

    private async Task RefreshCommands()
    {
        var currentTime = PointInTime.CreateEmpty();

        foreach (var batch in _commandsToRun.Items)
        {
            currentTime = await batch.RefreshResult(currentTime);
        }
    }

    private void RunWithLock(Action action) => Task.Run(async () => await RunWithLockAsync(action)).Wait();

    private async Task RunWithLockAsync(Action action)
    {
        await RunWithLockAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    private async Task RunWithLockAsync(Func<Task> func)
    {
        while (true)
        {
            lock (_guard)
            {
                if (!_resourceIsInUse)
                {
                    _resourceIsInUse = true;
                    break;
                }
            }

            await Task.Delay(1);
        }

        try
        {
            await func();
        }
        finally
        {
            lock (_guard)
            {
                _resourceIsInUse = false;
            }
        }
    }
}