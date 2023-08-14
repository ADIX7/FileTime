using System.Collections.ObjectModel;

namespace FileTime.App.Core.ViewModels.Timeline;

public interface ITimelineViewModel
{
    ObservableCollection<IParallelCommandsViewModel> ParallelCommandsGroups { get; }
}