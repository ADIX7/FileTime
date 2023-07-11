using FileTime.Core.Command.Create;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateContainer;

public class CreateContainerCommand : CreateItemBase
{
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public CreateContainerCommand(
        ITimelessContentProvider timelessContentProvider, 
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
        : base(timelessContentProvider, contentAccessorFactory)
    {
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    protected override async Task CreateItem(IItemCreator itemCreator, IItem resolvedParent)
    {
        await itemCreator.CreateContainerAsync(resolvedParent.Provider, Parent!.GetChild(NewItemName!));
        await _commandSchedulerNotifier.RefreshContainer(Parent);
    }
}