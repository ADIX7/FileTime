using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Move;

public class MoveCommandFactory
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    public MoveCommandFactory(
        IContentAccessorFactory contentAccessorFactory,
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    {
        _contentAccessorFactory = contentAccessorFactory;
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    public MoveCommand GenerateCommand(IEnumerable<ItemToMove> itemsToMove)
        => new(itemsToMove, _contentAccessorFactory, _timelessContentProvider, _commandSchedulerNotifier);
}