using System.Collections.ObjectModel;
using FileTime.Core.Timeline;
using ObservableComputations;

namespace FileTime.App.Core.ViewModels.Timeline;

public class TimelineViewModel : ITimelineViewModel, IDisposable
{
    private readonly OcConsumer _ocConsumer = new();
    public ObservableCollection<IParallelCommandsViewModel> ParallelCommandsGroups { get; }

    public TimelineViewModel(ICommandScheduler commandScheduler)
    {
        ParallelCommandsGroups =
            commandScheduler
                .CommandsToRun
                .Selecting(p => new ParallelCommandsViewModel(p) as IParallelCommandsViewModel)
                .For(_ocConsumer);
    }

    public void Dispose() => _ocConsumer.Dispose();
}