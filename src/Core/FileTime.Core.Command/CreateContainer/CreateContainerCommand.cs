using FileTime.Core.Command.Create;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateContainer;

public class CreateContainerCommand : CreateItemBase
{
    public CreateContainerCommand(ITimelessContentProvider timelessContentProvider, IContentAccessorFactory contentAccessorFactory)
        : base(timelessContentProvider, contentAccessorFactory)
    {
    }

    protected override async Task CreateItem(IItemCreator itemCreator, IItem resolvedParent)
    {
        await itemCreator.CreateContainerAsync(resolvedParent.Provider, Parent!.GetChild(NewItemName!));
    }
}