using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.Compress;

public class CompressCommandFactory(IUserCommunicationService userCommunicationService,
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    : ITransportationCommandFactory<CompressCommand>
{
    public CompressCommand GenerateCommand(IReadOnlyCollection<FullName> sources, TransportMode mode, FullName targetFullName)
        => new(
            userCommunicationService,
            timelessContentProvider,
            contentAccessorFactory,
            commandSchedulerNotifier,
            sources,
            mode,
            targetFullName
        );
}