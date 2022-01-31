using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class MoveCommand : ITransportationCommand
    {
        public IList<AbsolutePath>? Sources { get; } = new List<AbsolutePath>();

        public IContainer? Target { get; set; }
        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }
    }
}