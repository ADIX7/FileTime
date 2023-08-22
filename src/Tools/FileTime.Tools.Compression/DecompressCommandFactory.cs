using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression;

public class DecompressCommandFactory : ITransportationCommandFactory<DecompressCommand>
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public DecompressCommandFactory(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    public DecompressCommand GenerateCommand(IReadOnlyCollection<FullName> sources, TransportMode mode, FullName targetFullName)
        => new(
            _timelessContentProvider,
            _contentAccessorFactory,
            _commandSchedulerNotifier,
            sources,
            mode,
            targetFullName);
}