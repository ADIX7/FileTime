using FileTime.Core.ContentAccess;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Delete;

public class DeleteCommand : CommandBase, IExecutableCommand
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;
    public bool HardDelete { get; set; }
    public List<FullName> ItemsToDelete { get; } = new();

    public DeleteCommand(
        IContentAccessorFactory contentAccessorFactory,
        ITimelessContentProvider timelessContentProvider)
        : base("Delete")
    {
        _contentAccessorFactory = contentAccessorFactory;
        _timelessContentProvider = timelessContentProvider;
    }

    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO
        return Task.FromResult(CanCommandRun.True);
    }

    public override Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        //TODO
        return Task.FromResult(currentTime);
    }

    public async Task Execute()
    {
        //Calculate

        //Delete
        await TraverseTree(
            PointInTime.Present,
            ItemsToDelete,
            new Dictionary<string, IItemDeleter>(),
            new DeleteStrategy()
        );
    }

    private async Task TraverseTree(
        PointInTime currentTime,
        IEnumerable<FullName> itemsToDelete,
        Dictionary<string, IItemDeleter> itemDeleters,
        IDeleteStrategy deleteStrategy)
    {
        foreach (var itemToDeleteName in itemsToDelete)
        {
            var itemToDelete = await _timelessContentProvider.GetItemByFullNameAsync(itemToDeleteName, currentTime);
            IItemDeleter itemDeleter;

            if (itemDeleters.ContainsKey(itemToDelete.Provider.Name))
            {
                itemDeleter = itemDeleters[itemToDelete.Provider.Name];
            }
            else
            {
                itemDeleter = _contentAccessorFactory.GetItemDeleter(itemToDelete.Provider);
                itemDeleters.Add(itemToDelete.Provider.Name, itemDeleter);
            }

            if (itemToDelete is IContainer container)
            {
                await TraverseTree(
                    currentTime,
                    (await container.Items.GetItemsAsync())?.Select(i => i.Path) ?? Enumerable.Empty<FullName>(),
                    itemDeleters,
                    deleteStrategy
                );
            }

            await itemDeleter.DeleteAsync(itemToDelete.Provider, itemToDelete.FullName!);
        }
    }
}