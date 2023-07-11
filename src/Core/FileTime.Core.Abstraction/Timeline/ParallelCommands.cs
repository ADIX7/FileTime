using DynamicData;
using FileTime.Core.Command;
using FileTime.Core.Extensions;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public class ParallelCommands : IDisposable
{
    private static ushort _idCounter;
    private readonly SourceList<CommandTimeState> _commands;
    private PointInTime? _startTime;

    public ushort Id { get; }

    public IObservable<IChangeSet<CommandTimeState>> Commands { get; }

    public BindedCollection<CommandTimeState> CommandsCollection { get; }
    public int CommandCount => _commands.Count;

    public PointInTime? Result { get; private set; }

    public PointInTime? StartTime => _startTime;

    public async Task<PointInTime?> SetStartTimeAsync(PointInTime? startTime)
    {
        _startTime = startTime;
        return await RefreshResult();
    }

    public ParallelCommands(PointInTime? result)
        : this(new List<CommandTimeState>(), result)
    {
    }

    private ParallelCommands(List<CommandTimeState> commands, PointInTime? result)
    {
        Id = _idCounter++;

        _commands = new SourceList<CommandTimeState>();
        _commands.Edit((innerList) => innerList.AddRange(commands));
        Commands = _commands.Connect();
        CommandsCollection = Commands.ToBindedCollection();

        Result = result;
    }

    public static async Task<ParallelCommands> Create(PointInTime? startTime, IEnumerable<ICommand> commands)
    {
        var commandStates = new List<CommandTimeState>();
        var currentTime = startTime;
        foreach (var command in commands)
        {
            var commandTimeState = new CommandTimeState(command);
            await commandTimeState.UpdateStateAsync(currentTime);
            if (currentTime != null)
            {
                var canRun = await command.CanRun(currentTime);
                if (canRun == CanCommandRun.True)
                {
                    currentTime = await command.SimulateCommand(currentTime);
                }
                else
                {
                    currentTime = null;
                }
            }

            commandStates.Add(commandTimeState);
        }

        return new ParallelCommands(commandStates, currentTime);
    }

    public async Task AddCommand(ICommand command)
    {
        var commandTimeState = new CommandTimeState(command);
        await commandTimeState.UpdateStateAsync(Result);
        _commands.Add(commandTimeState);
        if (Result != null)
        {
            Result = await command.SimulateCommand(Result);
        }
    }

    public async Task RemoveCommand(ICommand command)
    {
        var commandTimeState = _commands.Items.First(c => c.Command == command);
        _commands.Remove(commandTimeState);
        await RefreshResult();
    }

    public async Task<PointInTime?> RefreshResult()
    {
        var result = StartTime;
        foreach (var commandTimeState in _commands.Items)
        {
            await commandTimeState.UpdateStateAsync(result);
            if (result != null)
            {
                var canRun = await commandTimeState.Command.CanRun(result);
                if (canRun == CanCommandRun.True || (canRun == CanCommandRun.Forcable && commandTimeState.ForceRun))
                {
                    result = await commandTimeState.Command.SimulateCommand(result);
                }
                else
                {
                    result = null;
                }
            }
        }

        Result = result;
        return Result;
    }

    public void Dispose()
    {
        CommandsCollection.Dispose();
    }
}