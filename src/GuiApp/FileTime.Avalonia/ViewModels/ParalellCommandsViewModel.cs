using System.Linq;
using System.Collections.Generic;
using System;
using FileTime.Core.Timeline;
using MvvmGen;
using System.Collections.ObjectModel;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    public partial class ParallelCommandsViewModel : IDisposable
    {
        private bool _disposed;

        [Property]
        private ObservableCollection<ParallelCommandViewModel> _parallelCommands;
        public ushort Id { get; }

        public ParallelCommandsViewModel(ReadOnlyParallelCommands parallelCommands)
        {
            _parallelCommands = new ObservableCollection<ParallelCommandViewModel>(parallelCommands.Commands.Select(c => new ParallelCommandViewModel(c)));
            Id = parallelCommands.Id;
        }

        ~ParallelCommandsViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                foreach(var commandVm in ParallelCommands)
                {
                    commandVm.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
