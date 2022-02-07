namespace FileTime.Core.Timeline
{
    public class CommandRunner
    {
        public Thread Thread { get; }
        public CommandTimeState Command { get; }

        public CommandRunner(Thread thread, CommandTimeState command)
        {
            Thread = thread;
            Command = command;
        }
    }
}