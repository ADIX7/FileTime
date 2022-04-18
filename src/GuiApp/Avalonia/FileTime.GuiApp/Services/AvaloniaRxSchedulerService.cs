using System.Reactive.Concurrency;
using FileTime.App.Core.Services;
using ReactiveUI;

namespace FileTime.GuiApp.Services
{
    public class AvaloniaRxSchedulerService : IRxSchedulerService
    {
        public IScheduler GetUIScheduler() => RxApp.MainThreadScheduler;

        public IScheduler GetWorkerScheduler() => RxApp.TaskpoolScheduler;
    }
}