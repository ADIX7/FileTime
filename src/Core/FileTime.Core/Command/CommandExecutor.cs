using FileTime.Core.Timeline;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Command
{
    public class CommandExecutor
    {
        private readonly List<ICommandHandler> _commandHandlers;
        private readonly ILogger<CommandExecutor> _logger;

        public CommandExecutor(
            IEnumerable<ICommandHandler> commandHandlers,
            ILogger<CommandExecutor> logger)
        {
            _commandHandlers = commandHandlers.ToList();
            _logger = logger;
        }

        public async Task ExecuteCommandAsync(ICommand command, TimeRunner timeRunner)
        {
            if (command is IExecutableCommand executableCommand)
            {
                await executableCommand.Execute(timeRunner);
            }
            else
            {
                var commandHandler = _commandHandlers.Find(c => c.CanHandle(command));
                if (commandHandler != null)
                {
                    await commandHandler.ExecuteAsync(command, timeRunner);
                }
                else
                {
                    _logger.LogError("No command handler for command {Command}", command.GetType().Name);
                }
            }
        }
    }
}