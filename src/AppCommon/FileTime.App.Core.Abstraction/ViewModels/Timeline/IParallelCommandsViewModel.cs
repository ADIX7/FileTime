using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public interface IParallelCommandsViewModel
{
    BindedCollection<ICommandTimeStateViewModel> Commands { get; }
}