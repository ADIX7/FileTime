using AsyncEvent;
using FileTime.Core.Command;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.Command
{
    public class DecompressCommand : IExecutableCommand
    {
        public string DisplayLabel => throw new NotImplementedException();

        public IReadOnlyList<string> CanRunMessages => throw new NotImplementedException();

        public int Progress => throw new NotImplementedException();

        public AsyncEventHandler ProgressChanged => throw new NotImplementedException();

        public int CurrentProgress => throw new NotImplementedException();

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }

        public Task Execute(TimeRunner timeRunner)
        {
            throw new NotImplementedException();
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }
    }
}