using AsyncEvent;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class RenameCommand : IExecutableCommand
    {
        public AbsolutePath Source { get; }
        public string Target { get; }

        public int Progress => 100;
        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; } = "RenameCommand";

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

        public async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var item = await Source.Resolve();
            if (item == null) throw new FileNotFoundException();
            var newDifferences = new List<Difference>()
            {
                new Difference(item.ToDifferenceItemType(),
                    DifferenceActionType.Delete,
                    Source),
                new Difference(item.ToDifferenceItemType(),
                    DifferenceActionType.Delete,
                    Source)
            };
            return startPoint.WithDifferences(newDifferences);
        }

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            return Task.FromResult(Source.Resolve() != null ? CanCommandRun.True : CanCommandRun.False);
        }
    }
}