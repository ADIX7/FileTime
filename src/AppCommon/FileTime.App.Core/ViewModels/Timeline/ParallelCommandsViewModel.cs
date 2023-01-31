using DynamicData.Alias;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public class ParallelCommandsViewModel : IParallelCommandsViewModel
{
    public BindedCollection<ICommandTimeStateViewModel> Commands { get; }

    public ParallelCommandsViewModel(ParallelCommands parallelCommands)
    {
        Commands = parallelCommands
            .Commands
            .Select(c => new CommandTimeStateViewModel(c) as ICommandTimeStateViewModel)
            .ToBindedCollection();
    }
}