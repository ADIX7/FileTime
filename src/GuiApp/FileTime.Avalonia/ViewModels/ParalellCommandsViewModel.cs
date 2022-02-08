using System.Linq;
using FileTime.Core.Timeline;
using MvvmGen;
using System.Collections.ObjectModel;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    public partial class ParallelCommandsViewModel
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

        public void Destroy()
        {
            if (!_disposed)
            {
                foreach(var commandVm in ParallelCommands)
                {
                    commandVm.Destroy();
                }
            }
            _disposed = true;
        }
    }
}
