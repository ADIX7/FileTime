using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy;

public class CopyStrategyFactory
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public CopyStrategyFactory(IContentAccessorFactory contentAccessorFactory)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }
    public ICopyStrategy CreateCopyStrategy(CopyFunc copy, List<OperationProgress> operationProgresses, Func<FullName, Task> refreshContainer)
    {
        return new CopyStrategy(copy, new CopyStrategyParam(operationProgresses, refreshContainer), _contentAccessorFactory);
    }
}