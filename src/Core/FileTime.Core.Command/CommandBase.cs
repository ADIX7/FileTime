using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public abstract class CommandBase : ICommand
{
    private readonly BehaviorSubject<string> _displayLabel;
    private readonly BehaviorSubject<int> _totalProgress;
    private readonly BehaviorSubject<int> _currentProgress;
    
    public IObservable<string> DisplayLabel { get; }
    public IObservable<int> TotalProgress { get; }
    public IObservable<int> CurrentProgress { get; }

    protected CommandBase(string displayLabel = "", int totalProgress = 0, int currentProgress = 0)
    {
        _displayLabel = new(displayLabel);
        _totalProgress = new(totalProgress);
        _currentProgress = new(currentProgress);
        
        DisplayLabel = _displayLabel.AsObservable();
        TotalProgress = _totalProgress.AsObservable();
        CurrentProgress = _currentProgress.AsObservable();
    }

    public abstract Task<CanCommandRun> CanRun(PointInTime currentTime);
    public abstract Task<PointInTime> SimulateCommand(PointInTime currentTime);
    
    protected void SetDisplayLabel(string displayLabel) => _displayLabel.OnNext(displayLabel);

    protected void SetTotalProgress(int totalProgress) => _totalProgress.OnNext(totalProgress);

    protected void SetCurrentProgress(int currentProgress) => _currentProgress.OnNext(currentProgress);

    protected IDisposable TrackProgress(IEnumerable<OperationProgress> operationProgresses) =>
        operationProgresses
            .Select(op => op.Progress.Select(p => (Progress: p, TotalProgress: op.TotalCount)))
            .CombineLatest()
            .Select(data =>
            {
                var total = data.Sum(d => d.TotalProgress);
                if (total == 0) return 0;
                return (int)(data.Sum(d => d.Progress) * 100 / total);
            })
            .Subscribe(SetTotalProgress);
}