using FileTime.App.Core.Command;

namespace FileTime.ConsoleUI.App.Command
{
    public class CommandBinding
    {
        private readonly Func<Task> _commandHandler;

        public string Name { get; }

        public ConsoleKeyInfo[] Keys { get; }
        public Commands Command { get; }

        public CommandBinding(string name, Commands command, ConsoleKeyInfo[] keys, Func<Task> commandHandler)
        {
            Name = name;
            Command = command;
            Keys = keys;
            _commandHandler = commandHandler;
        }

        public CommandBinding(string name, Commands command, ConsoleKeyInfo[] keys, Action commandHandler)
            : this(name, command, keys, () => { commandHandler(); return Task.CompletedTask; })
        {
        }

        public async Task InvokeAsync() => await _commandHandler();
    }
}