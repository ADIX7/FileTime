using FileTime.Core.Command;

namespace FileTime.Core.Timeline
{
    public class ReadOnlyCommandTimeState
    {
        public CanCommandRun CanRun { get; }
        public bool ForceRun { get; }
        public ICommand Command { get; }

        public ReadOnlyCommandTimeState(CommandTimeState commandTimeState)
        {
            CanRun = commandTimeState.CanRun;
            ForceRun = commandTimeState.ForceRun;
            Command = commandTimeState.Command;
        }
    }
}