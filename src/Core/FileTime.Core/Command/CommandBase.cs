using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public abstract class CommandBase : ICommand
    {
        private readonly List<string> _canRunMessages = new();
        public Dictionary<AbsolutePath, List<OperationProgress>> OperationStatuses { get; set; } = new();
        protected OperationProgress? CurrentOperationProgress { get; set; }
        public virtual string DisplayLabel { get; protected set; }
        public virtual IReadOnlyList<string> CanRunMessages { get; protected set; }
        public virtual int Progress { get; protected set; }
        public virtual int CurrentProgress { get; protected set; }
        public virtual AsyncEventHandler ProgressChanged { get; } = new AsyncEventHandler();

        public abstract Task<CanCommandRun> CanRun(PointInTime startPoint);
        public abstract Task<PointInTime> SimulateCommand(PointInTime startPoint);

        protected CommandBase()
        {
            CanRunMessages = _canRunMessages.AsReadOnly();
            DisplayLabel = "";
        }

        public async Task UpdateProgress()
        {
            var total = 0L;
            var current = 0L;

            foreach (var folder in OperationStatuses.Values)
            {
                foreach (var item in folder)
                {
                    current += item.Progress;
                    total += item.TotalCount;
                }
            }

            Progress = total == 0 ? 0 : (int)(current * 100 / total);
            if (CurrentOperationProgress == null)
            {
                CurrentProgress = 0;
            }
            else
            {
                CurrentProgress = CurrentOperationProgress.TotalCount == 0 ? 0 : (int)(CurrentOperationProgress.Progress * 100 / CurrentOperationProgress.TotalCount);
            }
            await ProgressChanged.InvokeAsync(this, AsyncEventArgs.Empty);
        }
    }
}