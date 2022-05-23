using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public class ParallelCommands
{
    private static ushort _idCounter;
    private List<CommandTimeState> _commands;
    public ushort Id { get; }
    public IReadOnlyList<CommandTimeState> Commands { get; }
    public PointInTime? Result { get; private set; }

    public ParallelCommands(PointInTime? result)
        : this(new List<CommandTimeState>(), result)
    {
    }

    private ParallelCommands(List<CommandTimeState> commands, PointInTime? result)
    {
        Id = _idCounter++;

        _commands = commands;
        Commands = _commands.AsReadOnly();

        Result = result;
    }

    public static async Task<ParallelCommands> Create(PointInTime? startTime, IEnumerable<ICommand> commands)
    {
        var commandStates = new List<CommandTimeState>();
        var currentTime = startTime;
        foreach (var command in commands)
        {
            CommandTimeState commandTimeState = new(command, currentTime);
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
        _commands.Add(new CommandTimeState(command, Result));
        if (Result != null)
        {
            Result = await command.SimulateCommand(Result);
        }
    }

    public async Task<PointInTime?> RefreshResult(PointInTime? startPoint)
    {
        var result = startPoint;
        foreach (var command in _commands)
        {
            await command.UpdateState(result);
            if (result != null)
            {
                var canRun = await command.Command.CanRun(result);
                if (canRun == CanCommandRun.True || (canRun == CanCommandRun.Forcable && command.ForceRun))
                {
                    result = await command.Command.SimulateCommand(result);
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

    public void RemoveAt(int number) => _commands.RemoveAt(number);

    internal void Remove(CommandTimeState command) => _commands.Remove(command);
}