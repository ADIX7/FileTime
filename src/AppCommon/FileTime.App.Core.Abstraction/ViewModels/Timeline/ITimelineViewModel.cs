using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels.Timeline;

public interface ITimelineViewModel
{
    BindedCollection<IParallelCommandsViewModel> ParallelCommandsGroups { get; }
}