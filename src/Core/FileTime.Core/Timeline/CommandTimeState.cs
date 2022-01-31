using FileTime.Core.Command;

namespace FileTime.Core.Timeline
{
    public class CommandTimeState
    {
        public ICommand Command { get; }
        public CanCommandRun CanRun { get; private set; } = CanCommandRun.False;
        public bool ForceRun { get; set; }
        public TimeProvider? TimeProvider { get; private set; }

        public CommandTimeState(ICommand command, PointInTime? startTime)
        {
            Command = command;
            UpdateState(startTime).Wait();
        }

        public async Task UpdateState(PointInTime? startPoint)
        {
            CanRun = startPoint == null ? CanCommandRun.False : await Command.CanRun(startPoint);
            if (startPoint != null)
            {
                TimeProvider = startPoint.Provider as TimeProvider;
            }
        }
    }
}