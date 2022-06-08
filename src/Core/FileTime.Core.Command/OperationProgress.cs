using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FileTime.Core.Command;

public class OperationProgress
{
    private readonly BehaviorSubject<long> _currentProgress = new(0);
    public string Key { get; }
    public IObservable<long> Progress { get; }
    public long TotalCount { get; }
    public IObservable<bool> IsDone { get; }

    public OperationProgress(string key, long totalCount)
    {
        Key = key;
        TotalCount = totalCount;

        Progress = _currentProgress.AsObservable();
        IsDone = Progress.Select(p => p >= TotalCount);
    }

    public void SetProgress(long progress) => _currentProgress.OnNext(progress);
}