using System.Linq;
using System.Collections.Generic;
using System;
using FileTime.Core.Timeline;

namespace FileTime.Avalonia.ViewModels
{
    public class ParallelCommandsViewModel : IDisposable
    {
        private bool _disposed;
        public IReadOnlyCollection<ParallelCommandViewModel> ParallelCommands { get; }
        public ushort Id { get; }

        public ParallelCommandsViewModel(ReadOnlyParallelCommands parallelCommands)
        {
            ParallelCommands = parallelCommands.Commands.Select(c => new ParallelCommandViewModel(c)).ToList().AsReadOnly();
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
