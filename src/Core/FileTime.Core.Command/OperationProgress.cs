using DeclarativeProperty;

namespace FileTime.Core.Command;

public class OperationProgress
{
    private readonly DeclarativeProperty<long> _currentProgress = new(0);
    public string Key { get; }
    public IDeclarativeProperty<long> Progress { get; }
    public long TotalCount { get; }
    public IDeclarativeProperty<bool> IsDone { get; }

    public OperationProgress(string key, long totalCount)
    {
        Key = key;
        TotalCount = totalCount;

        Progress = _currentProgress;
        IsDone = Progress.Map(p => p >= TotalCount);
    }

    public async Task SetProgressAsync(long progress) => await _currentProgress.SetValue(progress);

    public void SetProgressSafe(long progress) =>
        _currentProgress.SetValueSafe(progress);
}