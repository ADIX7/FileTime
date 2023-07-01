using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CopyCommandFactory : ITransportationCommandFactory<CopyCommand>
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public CopyCommandFactory(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)

    {
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    public CopyCommand GenerateCommand(
        IReadOnlyCollection<FullName> sources,
        TransportMode mode,
        FullName targetFullName) 
        => new(_timelessContentProvider, _commandSchedulerNotifier, sources, mode, targetFullName);
}