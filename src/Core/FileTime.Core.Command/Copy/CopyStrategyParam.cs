using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy;

public class CopyStrategyParam
{
    private readonly Func<FullName, Task> _refreshContainer;
    public List<OperationProgress> OperationProgresses { get; }

    public CopyStrategyParam(List<OperationProgress> operationProgresses, Func<FullName, Task> refreshContainer)
    {
        OperationProgresses = operationProgresses;
        _refreshContainer = refreshContainer;
    }

    public async Task RefreshContainerAsync(FullName containerName) => await _refreshContainer(containerName);
}