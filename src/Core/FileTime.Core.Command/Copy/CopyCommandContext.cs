namespace FileTime.Core.Command.Copy;

public class CopyCommandContext
{
    private readonly Func<Task> _updateProgress;

    public CopyCommandContext(
        Func<Task> updateProgress,
        OperationProgress? currentProgress,
        CancellationToken cancellationToken)
    {
        _updateProgress = updateProgress;
        CancellationToken = cancellationToken;
        CurrentProgress = currentProgress;
    }

    public OperationProgress? CurrentProgress { get; }
    public CancellationToken CancellationToken { get; }

    public async Task UpdateProgressAsync() => await _updateProgress.Invoke();
}