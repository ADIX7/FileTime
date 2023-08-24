using Microsoft.Extensions.Logging;

namespace FileTime.Core.Command;

public class CommandRunner : ICommandRunner
{
    private readonly List<ICommandHandler> _commandHandlers;
    private readonly ILogger<CommandRunner> _logger;

    public CommandRunner(
        IEnumerable<ICommandHandler> commandHandlers,
        ILogger<CommandRunner> logger)
    {
        _commandHandlers = commandHandlers.ToList();
        _logger = logger;
    }

    public async Task RunCommandAsync(ICommand command)
    {
        if (command is IExecutableCommand executableCommand)
        {
            await executableCommand.Execute();
        }
        else
        {
            var commandHandler = await _commandHandlers
                .ToAsyncEnumerable()
                .FirstOrDefaultAwaitAsync(async c => await c.CanHandleAsync(command));
            
            if (commandHandler != null)
            {
                await commandHandler.ExecuteAsync(command);
            }
            else
            {
                _logger.LogError("No command handler for command {Command}", command.GetType().Name);
            }
        }
    }
}