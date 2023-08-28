using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CalculateStrategy : ICopyStrategy
{
    private readonly List<OperationProgress> _operationStatuses;
    public IReadOnlyList<OperationProgress> OperationStatuses { get; }

    public CalculateStrategy()
    {
        _operationStatuses = new();
        OperationStatuses = _operationStatuses.AsReadOnly();
    }

    public Task ContainerCopyDoneAsync(AbsolutePath path) => Task.CompletedTask;

    public async Task CopyAsync(AbsolutePath from, AbsolutePath to, CopyCommandContext context)
    {
        var resolvedFrom = await from.ResolveAsync();
        _operationStatuses.Add(new OperationProgress(from.Path.Path, (resolvedFrom as IElement)?.Size ?? 0L));
    }

    public Task CreateContainerAsync(IContainer target, string name, PointInTime currentTime) => Task.CompletedTask;
}