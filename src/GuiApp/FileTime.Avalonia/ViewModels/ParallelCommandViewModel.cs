using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncEvent;
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

        [Property]
        private int _progress;

        [Property]
        private bool _isSelected;

        public CanCommandRun CanRun => _commandTimeState.CanRun;
        public bool ForceRun => _commandTimeState.ForceRun;

        public string Name => _commandTimeState.Command.DisplayLabel;

        public ParallelCommandViewModel(ReadOnlyCommandTimeState commandTimeState)
        {
            _commandTimeState = commandTimeState;
            _commandTimeState.Command.ProgressChanged.Add(HandleProgressChange);
        }

        private Task HandleProgressChange(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            Progress = _commandTimeState.Command.Progress;
            return Task.CompletedTask;
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