using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Rename
{
    public class RenameCommand : IExecutableCommand
    {
        public AbsolutePath Source { get; }
        public string Target { get; }

        public int Progress => 100;
        public int CurrentProgress => 100;
        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; } = "RenameCommand";
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public RenameCommand(AbsolutePath source, string target)
        {
            Source = source;
            Target = target;
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            var itemToRename = await Source.ResolveAsync();
            if (itemToRename != null)
            {
                await itemToRename.Rename(Target);
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(itemToRename.GetParent()!));
            }
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>()
            {
                new Difference(
                    DifferenceActionType.Delete,
                    Source),
                new Difference(
                    DifferenceActionType.Create,
                    Source.GetParent().GetChild(Target, Source.Type))
            };
            return Task.FromResult(startPoint.WithDifferences(newDifferences));
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            return await Source.ResolveAsync() != null ? CanCommandRun.True : CanCommandRun.False;
        }
    }
}