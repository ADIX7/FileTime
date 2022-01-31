using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class RenameCommand : IExecutableCommand
    {
        public AbsolutePath Source { get; }
        public string Target { get; }

        public RenameCommand(AbsolutePath source, string target)
        {
            Source = source;
            Target = target;
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            var itemToRename = await Source.Resolve();
            if (itemToRename != null)
            {
                await itemToRename.Rename(Target);
                timeRunner.RefreshContainer?.InvokeAsync(this, new AbsolutePath(itemToRename.GetParent()!));
            }
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            throw new NotImplementedException();
        }
    }
}