using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class SimulateStrategy : ICopyStrategy
{
    private readonly List<Difference> _newDiffs;
    private readonly ITimelessContentProvider _timelessContentProvider;

    public IReadOnlyList<Difference> NewDiffs { get; }

    public SimulateStrategy(ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
        _newDiffs = new();
        NewDiffs = _newDiffs.AsReadOnly();
    }

    public Task CreateContainerAsync(IContainer target, string name, PointInTime currentTime)
    {
        var newContainerDiff = new Difference(
            DifferenceActionType.Create,
            new AbsolutePath(_timelessContentProvider, currentTime, target.FullName!.GetChild(name), Enums.AbsolutePathType.Container)
        );

        _newDiffs.Add(newContainerDiff);

        return Task.FromResult((IContainer)null!);
    }

    public Task ContainerCopyDoneAsync(AbsolutePath path) => Task.CompletedTask;

    public Task CopyAsync(AbsolutePath from, AbsolutePath to, CopyCommandContext context)
    {
        _newDiffs.Add(new Difference(DifferenceActionType.Create, to));
        return Task.CompletedTask;
    }
}