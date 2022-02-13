using AsyncEvent;
using FileTime.Core.Command;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.Command
{
    public class CompressCommand : IExecutableCommand
    {
        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();
        public string DisplayLabel { get; } = "Compress";

        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public int Progress { get; }

        public AsyncEventHandler ProgressChanged { get; } = new AsyncEventHandler();

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            //TODO: implement
            return Task.FromResult(CanCommandRun.True);
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            return Task.FromResult(startPoint.WithDifferences(new List<Difference>()));
        }

        public Task Execute(TimeRunner timeRunner)
        {
            throw new NotImplementedException();
        }
    }
}