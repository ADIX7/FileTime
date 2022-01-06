using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class MoveCommand : ITransportationCommand
    {
        public IList<IAbsolutePath> Sources { get; } = new List<IAbsolutePath>();

        public IContainer? Target { get; set; }
        public TransportMode TransportMode { get; set; } = TransportMode.Merge;

        public PointInTime SimulateCommand(PointInTime delta)
        {
            throw new NotImplementedException();
        }
    }
}