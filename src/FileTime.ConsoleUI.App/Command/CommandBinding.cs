namespace FileTime.ConsoleUI.App.Command
{
    public class CommandBinding
    {
        private readonly Action _commandHandler;

        public string Name { get; }

        public ConsoleKeyInfo[] Keys { get; }
        public Commands Command { get; }

        public CommandBinding(string name, Commands command, ConsoleKeyInfo[] keys, Action commandHandler)
        {
            Name = name;
            Command = command;
            Keys = keys;
            _commandHandler = commandHandler;
        }

        public void Invoke() => _commandHandler();
    }
}