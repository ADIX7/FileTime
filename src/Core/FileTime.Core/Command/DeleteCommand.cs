using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class DeleteCommand : IExecutableCommand
    {
        public IList<IAbsolutePath> ItemsToDelete { get; } = new List<IAbsolutePath>();

        public PointInTime SimulateCommand(PointInTime delta)
        {
            throw new NotImplementedException();
        }

        public async Task Execute()
        {
            foreach (var item in ItemsToDelete)
            {
                await DoDelete(await item.ContentProvider.GetByPath(item.Path)!);
            }
        }

        private async Task DoDelete(IItem item)
        {
            if (item is IContainer container)
            {
                foreach (var child in await container.GetItems())
                {
                    await DoDelete(child);
                    await child.Delete();
                }

                await item.Delete();
            }
            else if(item is IElement element)
            {
                await element.Delete();
            }
        }
    }
}