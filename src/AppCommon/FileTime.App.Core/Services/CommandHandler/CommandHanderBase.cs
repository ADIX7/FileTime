using FileTime.App.Core.Command;

namespace FileTime.App.Core.Services.CommandHandler
{
    public abstract class CommandHanderBase : ICommandHandler
    {
        private readonly Dictionary<Commands, Func<Task>> _commandHandlers = new();
        public bool CanHandleCommand(Commands command) => _commandHandlers.ContainsKey(command);

        public async Task HandleCommandAsync(Commands command) => await _commandHandlers[command].Invoke();

        protected void AddCommandHandler(Commands command, Func<Task> handler) => _commandHandlers.Add(command, handler);
        protected void AddCommandHandlers(IEnumerable<(Commands command, Func<Task> handler)> commandHandlers)
        {
            foreach (var (command, handler) in commandHandlers)
            {
                AddCommandHandler(command, handler);
            }
        }

        protected void RemoveCommandHandler(Commands command) => _commandHandlers.Remove(command);
    }
}