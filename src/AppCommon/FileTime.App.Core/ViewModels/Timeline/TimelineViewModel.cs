using DynamicData.Alias;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public class TimelineViewModel : ITimelineViewModel
{
    public BindedCollection<IParallelCommandsViewModel> ParallelCommandsGroups { get; }

    public TimelineViewModel(ICommandScheduler commandScheduler)
    {
        ParallelCommandsGroups =
            commandScheduler
                .CommandsToRun
                .Select(p => new ParallelCommandsViewModel(p) as IParallelCommandsViewModel)
                .ToBindedCollection();
    }
}