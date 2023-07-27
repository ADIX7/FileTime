namespace FileTime.Core.Command.Copy;

public class CopyCommandContext
{
    private readonly Func<Task> _updateProgress;

    public CopyCommandContext(Func<Task> updateProgress, OperationProgress? currentProgress)
    {
        _updateProgress = updateProgress;
        CurrentProgress = currentProgress;
    }

    public OperationProgress? CurrentProgress { get; }

    public async Task UpdateProgressAsync() => await _updateProgress.Invoke();
}