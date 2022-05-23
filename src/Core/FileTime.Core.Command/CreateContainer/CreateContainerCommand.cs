using FileTime.Core.Enums;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateContainer;

public class CreateContainerCommand : IExecutableCommand
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    public FullName Parent { get; }
    public string NewContainerName { get; }

    public CreateContainerCommand(
        FullName parent,
        string newContainerName,
        ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
        Parent = parent;
        NewContainerName = newContainerName;
    }

    public async Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
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

    public Task Execute()
    {
        return Task.CompletedTask;
    }

    private async Task<IItem> ResolveParentAsync()
        => await _timelessContentProvider.GetItemByFullNameAsync(Parent, PointInTime.Present);
}