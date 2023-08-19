using DeclarativeProperty;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public abstract class CommandBase : ICommand
{
    private readonly DeclarativeProperty<string> _displayLabel;
    private readonly DeclarativeProperty<string> _displayDetailLabel;
    private readonly DeclarativeProperty<int> _totalProgress;
    private readonly DeclarativeProperty<int> _currentProgress;

    public IDeclarativeProperty<string> DisplayLabel { get; }
    public IDeclarativeProperty<string> DisplayDetailLabel { get; }
    public IDeclarativeProperty<int> TotalProgress { get; }
    public IDeclarativeProperty<int> CurrentProgress { get; }

    protected CommandBase(string displayLabel = "", string displayDetailLabel = "", int totalProgress = 0, int currentProgress = 0)
    {
        _displayLabel = new(displayLabel);
        _displayDetailLabel = new(displayDetailLabel);
        _totalProgress = new(totalProgress);
        _currentProgress = new(currentProgress);

        DisplayLabel = _displayLabel;
        DisplayDetailLabel = _displayDetailLabel;
        TotalProgress = _totalProgress;
        CurrentProgress = _currentProgress;
    }

    public abstract Task<CanCommandRun> CanRun(PointInTime currentTime);
    public abstract Task<PointInTime> SimulateCommand(PointInTime currentTime);
    public abstract void Cancel();

    protected async Task SetDisplayLabelAsync(string? displayLabel) => await _displayLabel.SetValue(displayLabel ?? string.Empty);
    protected async Task SetDisplayDetailLabel(string? displayLabel) => await _displayDetailLabel.SetValue(displayLabel ?? string.Empty);

    protected async Task SetTotalProgress(int totalProgress) => await _totalProgress.SetValue(totalProgress);

    protected async Task SetCurrentProgress(int currentProgress) => await _currentProgress.SetValue(currentProgress);

    protected IDisposable TrackProgress(IEnumerable<OperationProgress> operationProgresses) =>
        operationProgresses
            .Select(op => op.Progress.Map(p => (Progress: p, TotalProgress: op.TotalCount)))
            .CombineAll(data =>
            {
                var dataList = data.ToList();
                var total = dataList.Sum(d => d.TotalProgress);
                if (total == 0) return Task.FromResult(0);
                return Task.FromResult((int)(dataList.Sum(d => d.Progress) * 100 / total));
            })
            .Subscribe(async (p, _) => await SetTotalProgress(p));
}