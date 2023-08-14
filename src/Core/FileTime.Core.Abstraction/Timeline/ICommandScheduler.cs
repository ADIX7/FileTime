using System.Collections.ObjectModel;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public interface ICommandScheduler
{
    Task AddCommand(ICommand command, int? batchId = null, bool toNewBatch = false);
    IObservable<FullName> ContainerToRefresh { get; }
    ReadOnlyObservableCollection<ParallelCommands> CommandsToRun { get; }
    bool IsRunningEnabled { get; }
    void RefreshContainer(FullName container);
    Task SetRunningEnabledAsync(bool value);
}