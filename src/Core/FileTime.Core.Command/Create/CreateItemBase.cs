using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using InitableService;

namespace FileTime.Core.Command.Create;

public abstract class CreateItemBase : CommandBase, IExecutableCommand, IInitable<FullName, string>
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    public FullName? Parent { get; private set; }
    public string? NewItemName { get; private set; }

    protected CreateItemBase(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
        : base("Create")
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
    }

    public override async Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        if (Parent is null)
        {
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        if (NewItemName is null)
        {
            throw new ArgumentNullException(nameof(NewItemName),
                $"Property {nameof(NewItemName)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        try
        {
            var parent = await ResolveParentAsync();
            if (parent is not IContainer parentContainer) return CanCommandRun.False;

            var items = parentContainer.ItemsCollection;
            var existingItem = items.FirstOrDefault(i => i.Path.GetName() == NewItemName);

            return existingItem switch
            {
                null => CanCommandRun.True,
                {Type: AbsolutePathType.Container} => CanCommandRun.Forcable,
                _ => CanCommandRun.False
            };
        }
        catch
        {
        }

        return CanCommandRun.False;
    }

    public override Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        if (Parent is null)
        {
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        if (NewItemName is null)
        {
            throw new ArgumentNullException(nameof(NewItemName),
                $"Property {nameof(NewItemName)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        return Task.FromResult(
            currentTime.WithDifferences(newPointInTime =>
                new List<Difference>()
                {
                    new(
                        DifferenceActionType.Create,
                        new AbsolutePath(_timelessContentProvider,
                            newPointInTime,
                            Parent.GetChild(NewItemName),
                            AbsolutePathType.Container
                        )
                    )
                }
            )
        );
    }

    public async Task Execute()
    {
        if (Parent is null)
        {
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        if (NewItemName is null)
        {
            throw new ArgumentNullException(nameof(NewItemName),
                $"Property {nameof(NewItemName)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        var resolvedParent = await _timelessContentProvider.GetItemByFullNameAsync(Parent, PointInTime.Present);
        var itemCreator = _contentAccessorFactory.GetItemCreator(resolvedParent.Provider);
        await CreateItem(itemCreator, resolvedParent);
    }

    abstract protected Task CreateItem(IItemCreator itemCreator, IItem resolvedParent);

    private async Task<IItem> ResolveParentAsync()
    {
        if (Parent is null)
        {
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        if (NewItemName is null)
        {
            throw new ArgumentNullException(nameof(NewItemName),
                $"Property {nameof(NewItemName)} is not initialized. Call the {nameof(Init)} method before using the command.");
        }

        return await _timelessContentProvider.GetItemByFullNameAsync(Parent, PointInTime.Present);
    }

    public void Init(FullName parent, string newContainerName)
    {
        Parent = parent;
        NewItemName = newContainerName;
    }
}