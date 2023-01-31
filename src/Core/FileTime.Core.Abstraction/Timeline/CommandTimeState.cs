using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public class CommandTimeState
{
    private object _executionStateLock = new object();
    private ExecutionState _executionState;
    public ICommand Command { get; }
    public CanCommandRun CanRun { get; private set; } = CanCommandRun.False;
    public bool ForceRun { get; set; }

    public ExecutionState ExecutionState
    {
        get
        {
            lock (_executionStateLock)
            {
                return _executionState;
            }
        }
        set
        {
            lock (_executionStateLock)
            {
                _executionState = value;
            }
        }
    }

    public CommandTimeState(ICommand command)
    {
        Command = command;
        SetToWait();
    }

    public async Task UpdateStateAsync(PointInTime? startPoint)
    {
        CanRun = startPoint == null ? CanCommandRun.False : await Command.CanRun(startPoint);
    }

    private async void SetToWait()
    {
        // This command won't be shown in the scheduler until this timeout is over
        // Short living commands while not "blink on" the list 
        await Task.Delay(100);
        lock (_executionStateLock)
        {
            if (_executionState == ExecutionState.Initializing)
                _executionState = ExecutionState.Waiting;
        }
    }
}