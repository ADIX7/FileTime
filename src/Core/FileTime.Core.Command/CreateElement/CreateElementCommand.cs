using FileTime.Core.Command.Create;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateElement;

public class CreateElementCommand : CreateItemBase
{
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public CreateElementCommand(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
        : base(timelessContentProvider, contentAccessorFactory)
    {
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    protected override async Task CreateItem(IItemCreator itemCreator, IItem resolvedParent)
    {
        await itemCreator.CreateElementAsync(resolvedParent.Provider, Parent!.GetChild(NewItemName!));
        await _commandSchedulerNotifier.RefreshContainer(Parent);
    }

    public override void Cancel()
    {
    }
}