using AsyncEvent;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class MoveCommand : ITransportationCommand
    {
        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();

        public AbsolutePath? Target { get; set; }
        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;

        public int Progress => 100;
        public int CurrentProgress => 100;
        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; } = "MoveCommand";
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();
        public bool TargetIsContainer => true;

        public List<InputElement> Inputs { get; } = new();
        public List<object>? InputResults { get; set; }

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