using AsyncEvent;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class DeleteCommand : IExecutableCommand
    {
        public int Progress => 100;

        public AsyncEventHandler ProgressChanged { get; } = new();

        public IList<AbsolutePath> ItemsToDelete { get; } = new List<AbsolutePath>();
        public string DisplayLabel { get; } = "DeleteCommand";

        public async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>();

            foreach (var itemToDelete in ItemsToDelete)
            {
                var item = await itemToDelete.Resolve();
                newDifferences.Add(new Difference(
                    item.ToDifferenceItemType(),
                    DifferenceActionType.Delete,
                    itemToDelete
                ));
            }
            return startPoint.WithDifferences(newDifferences);
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            foreach (var item in ItemsToDelete)
            {
                await DoDelete((await item.Resolve())!, timeRunner);
            }
        }

        private async Task DoDelete(IItem item, TimeRunner timeRunner)
        {
            if (item is IContainer container)
            {
                foreach (var child in (await container.GetItems())!)
                {
                    await DoDelete(child, timeRunner);
                    await child.Delete();
                }

                await item.Delete();
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(container));
            }
            else if (item is IElement element)
            {
                await element.Delete();
            }
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            var result = CanCommandRun.True;
            foreach (var itemPath in ItemsToDelete)
            {
                var resolvedItem = await itemPath.Resolve();
                if (!(resolvedItem?.CanDelete ?? true))
                {
                    result = CanCommandRun.Forceable;
                }
            }

            return result;
        }
    }
}