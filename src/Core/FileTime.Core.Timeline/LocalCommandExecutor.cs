using FileTime.Core.Command;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Timeline;

public class LocalCommandExecutor : ILocalCommandExecutor
{
    private readonly ICommandRunner _commandRunner;
    private readonly ILogger<LocalCommandExecutor> _logger;
    public event EventHandler<ICommand>? CommandFinished;

    public LocalCommandExecutor(ICommandRunner commandRunner, ILogger<LocalCommandExecutor> logger)
    {
        _commandRunner = commandRunner;
        _logger = logger;
    }

    public void ExecuteCommand(ICommand command)
    {
        var context = new CommandRunnerContext(command);
        var thread = new Thread(RunCommand);
        thread.Start(context);
    }

    private async void RunCommand(object? contextObj)
    {
        if (contextObj is not CommandRunnerContext context)
            throw new ArgumentException($"Parameter must be of type {typeof(CommandRunnerContext)}");
        try
        {
            await _commandRunner.RunCommandAsync(context.Command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {Command}", context.Command.GetType().Name);
        }

        CommandFinished?.Invoke(this, context.Command);
    }
}