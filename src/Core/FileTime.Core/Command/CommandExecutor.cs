namespace FileTime.Core.Command
{
    public class CommandExecutor
    {
        private readonly List<ICommandHandler> _commandHandlers;

        public CommandExecutor(IEnumerable<ICommandHandler> commandHandlers)
        {
            _commandHandlers = commandHandlers.ToList();
        }

        public void ExecuteCommand(ICommand command)
        {
            if (command is IExecutableCommand executableCommand)
            {
                executableCommand.Execute();
            }
            else
            {
                _commandHandlers.Find(c => c.CanHandle(command))?.Execute(command);
            }
        }
    }
}