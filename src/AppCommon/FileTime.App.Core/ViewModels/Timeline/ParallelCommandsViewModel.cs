using System.Collections.ObjectModel;
using FileTime.Core.Timeline;
using ObservableComputations;

namespace FileTime.App.Core.ViewModels.Timeline;

public class ParallelCommandsViewModel : IParallelCommandsViewModel, IDisposable
{
    private readonly OcConsumer _ocConsumer = new();
    public ObservableCollection<ICommandTimeStateViewModel> Commands { get; }

    public ParallelCommandsViewModel(ParallelCommands parallelCommands)
    {
        Commands = parallelCommands
            .Commands
            .Selecting(c => new CommandTimeStateViewModel(c) as ICommandTimeStateViewModel)
            .For(_ocConsumer);
    }

    public void Dispose() => _ocConsumer.Dispose();
}