using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Delete;

public class DeleteCommand : CommandBase, IExecutableCommand
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    public bool HardDelete { get; set; }
    public List<FullName> ItemsToDelete { get; } = new();

    public DeleteCommand(
        IContentAccessorFactory contentAccessorFactory,
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)
        : base("Delete")
    {
        _contentAccessorFactory = contentAccessorFactory;
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
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

    public override void Cancel()
    {
        //TODO: Implement
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

        var parents = ItemsToDelete.Select(i => i.GetParent()).OfType<FullName>().Distinct();
        foreach (var parent in parents)
        {
            await _commandSchedulerNotifier.RefreshContainer(parent);
        }
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
                    container.Items.Select(i => i.Path),
                    itemDeleters,
                    deleteStrategy
                );

                if (container.FullName is not null)
                {
                    await _commandSchedulerNotifier.RefreshContainer(container.FullName);
                }
            }

            await itemDeleter.DeleteAsync(itemToDelete.Provider, itemToDelete.FullName!);
        }
    }
}