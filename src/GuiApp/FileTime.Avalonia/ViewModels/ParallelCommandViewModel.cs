using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AsyncEvent;
using Avalonia.Threading;
using FileTime.Core.Command;
using FileTime.Core.Timeline;
using MvvmGen;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    public partial class ParallelCommandViewModel
    {
        private bool _disposed;

        [Property]
        private ReadOnlyCommandTimeState _commandTimeState;

        private readonly BehaviorSubject<int> _progressSubject = new BehaviorSubject<int>(0);

        [Property]
        private IObservable<int> _progress;

        [Property]
        private bool _isSelected;

        public CanCommandRun CanRun => _commandTimeState.CanRun;
        public bool ForceRun => _commandTimeState.ForceRun;

        public string Name => _commandTimeState.Command.DisplayLabel;

        public ParallelCommandViewModel(ReadOnlyCommandTimeState commandTimeState)
        {
            _commandTimeState = commandTimeState;
            _commandTimeState.Command.ProgressChanged.Add(HandleProgressChange);

            _progress = _progressSubject.Throttle(TimeSpan.FromSeconds(1));
        }

        private async Task HandleProgressChange(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _progressSubject.OnNext(_commandTimeState.Command.Progress));
        }

        public void Destroy()
        {
            if (!_disposed)
            {
                _commandTimeState.Command.ProgressChanged.Remove(HandleProgressChange);
            }
            _disposed = true;
        }
    }
}