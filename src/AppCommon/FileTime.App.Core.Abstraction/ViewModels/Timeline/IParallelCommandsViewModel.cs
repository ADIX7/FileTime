using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels.Timeline;

public interface IParallelCommandsViewModel
{
    BindedCollection<ICommandTimeStateViewModel> Commands { get; }
}