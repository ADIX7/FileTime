using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using InitableService;

namespace FileTime.Core.Command.CreateContainer;

public class CreateContainerCommand : IExecutableCommand, IInitable<FullName, string>
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    public FullName? Parent { get; private set; }
    public string? NewContainerName { get; private set; }

    public CreateContainerCommand(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
    }

    public async Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        if (Parent is null)
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        if (NewContainerName is null)
            throw new ArgumentNullException(nameof(NewContainerName),
                $"Property {nameof(NewContainerName)} is not initialized. Call the {nameof(Init)} method before using the command.");

        try
        {
            var parent = await ResolveParentAsync();
            if (parent is not IContainer parentContainer) return CanCommandRun.False;

            var items = await parentContainer.Items.GetItemsAsync();
            if (items is null) return CanCommandRun.Forcable;

            var existingItem = items.FirstOrDefault(i => i.Path.GetName() == NewContainerName);

            return existingItem switch
            {
                null => CanCommandRun.True,
                { Type: AbsolutePathType.Container } => CanCommandRun.Forcable,
                _ => CanCommandRun.False
            };
        }
        catch
        {
        }

        return CanCommandRun.False;
    }

    public Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        if (Parent is null)
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        if (NewContainerName is null)
            throw new ArgumentNullException(nameof(NewContainerName),
                $"Property {nameof(NewContainerName)} is not initialized. Call the {nameof(Init)} method before using the command.");

        return Task.FromResult(
            currentTime.WithDifferences(newPointInTime =>
                new List<Difference>()
                {
                    new(
                        DifferenceActionType.Create,
                        new AbsolutePath(_timelessContentProvider,
                            newPointInTime,
                            Parent.GetChild(NewContainerName),
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
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        if (NewContainerName is null)
            throw new ArgumentNullException(nameof(NewContainerName),
                $"Property {nameof(NewContainerName)} is not initialized. Call the {nameof(Init)} method before using the command.");

        var resolvedParent = await _timelessContentProvider.GetItemByFullNameAsync(Parent, PointInTime.Present);
        var itemCreator = _contentAccessorFactory.GetItemCreator(resolvedParent.Provider);
        await itemCreator.CreateContainerAsync(resolvedParent.Provider, Parent.GetChild(NewContainerName));
    }

    private async Task<IItem> ResolveParentAsync()
    {
        if (Parent is null)
            throw new ArgumentNullException(nameof(Parent),
                $"Property {nameof(Parent)} is not initialized. Call the {nameof(Init)} method before using the command.");
        if (NewContainerName is null)
            throw new ArgumentNullException(nameof(NewContainerName),
                $"Property {nameof(NewContainerName)} is not initialized. Call the {nameof(Init)} method before using the command.");

        return await _timelessContentProvider.GetItemByFullNameAsync(Parent, PointInTime.Present);
    }

    public void Init(FullName parent, string newContainerName)
    {
        Parent = parent;
        NewContainerName = newContainerName;
    }
}