using System.Reactive.Concurrency;

namespace FileTime.App.Core.Services;

public interface IRxSchedulerService
{
    IScheduler GetWorkerScheduler();
    IScheduler GetUIScheduler();
}