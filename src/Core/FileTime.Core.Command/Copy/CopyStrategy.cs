using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CopyStrategy : ICopyStrategy
{
    private readonly CopyFunc _copy;
    private readonly CopyStrategyParam _copyStrategyParam;

    public CopyStrategy(CopyFunc copy, CopyStrategyParam copyStrategyParam)
    {
        _copy = copy;
        _copyStrategyParam = copyStrategyParam;
    }

    public async Task ContainerCopyDoneAsync(AbsolutePath containerPath)
    {
        foreach (var item in _copyStrategyParam.OperationProgresses.FindAll(o => o.Key.StartsWith(containerPath.Path.Path)))
        {
            await item.SetProgressAsync(item.TotalCount);
        }

        await _copyStrategyParam.RefreshContainerAsync(containerPath.Path);
    }

    public async Task CopyAsync(AbsolutePath from, AbsolutePath to, CopyCommandContext context)
    {
        await _copy(from, to, context);
        if (context.CurrentProgress is not null)
        {
            await context.CurrentProgress.SetProgressAsync(context.CurrentProgress.TotalCount);
        }

        if (to.Path.GetParent() is { } parent)
            await _copyStrategyParam.RefreshContainerAsync(parent);
    }

    public Task CreateContainerAsync(IContainer target, string name, PointInTime currentTime)
    {
        throw new NotImplementedException();
    }
}