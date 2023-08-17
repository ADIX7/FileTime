using FileTime.Core.Command;
using FileTime.Core.ContentAccess;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression;

public class CompressCommandFactory : ITransportationCommandFactory<CompressCommand>
{
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public CompressCommandFactory(
        IUserCommunicationService userCommunicationService,
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    {
        _userCommunicationService = userCommunicationService;
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    public CompressCommand GenerateCommand(IReadOnlyCollection<FullName> sources, TransportMode mode, FullName targetFullName)
        => new(
            _userCommunicationService,
            _timelessContentProvider,
            _contentAccessorFactory,
            _commandSchedulerNotifier,
            sources,
            mode,
            targetFullName
        );
}