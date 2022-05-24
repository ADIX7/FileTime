using FileTime.Core.Command.Create;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateElement;

public class CreateElementCommand : CreateItemBase
{
    public CreateElementCommand(ITimelessContentProvider timelessContentProvider, IContentAccessorFactory contentAccessorFactory)
        : base(timelessContentProvider, contentAccessorFactory)
    {
    }

    protected override async Task CreateItem(IItemCreator itemCreator, IItem resolvedParent)
    {
        await itemCreator.CreateElementAsync(resolvedParent.Provider, Parent!.GetChild(NewItemName!));
    }
}