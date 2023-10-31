using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.Decompress;

public class DecompressCommandFactory(ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    : ITransportationCommandFactory<DecompressCommand>
{
    public DecompressCommand GenerateCommand(IReadOnlyCollection<FullName> sources, TransportMode mode, FullName targetFullName)
        => new(
            timelessContentProvider,
            contentAccessorFactory,
            commandSchedulerNotifier,
            sources,
            mode,
            targetFullName);
}