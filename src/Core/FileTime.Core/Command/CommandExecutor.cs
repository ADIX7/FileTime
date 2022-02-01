using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CommandExecutor
    {
        private readonly List<ICommandHandler> _commandHandlers;

        public CommandExecutor(IEnumerable<ICommandHandler> commandHandlers)
        {
            _commandHandlers = commandHandlers.ToList();
        }

        public async Task ExecuteCommandAsync(ICommand command, TimeRunner timeRunner)
        {
            if (command is IExecutableCommand executableCommand)
            {
                await executableCommand.Execute(timeRunner);
            }
            else
            {
                await _commandHandlers.Find(c => c.CanHandle(command))?.ExecuteAsync(command, timeRunner);
            }
        }
    }
}