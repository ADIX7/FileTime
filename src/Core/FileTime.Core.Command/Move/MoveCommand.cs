using System.Reactive.Subjects;
using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Move;

public class MoveCommand : CommandBase, IExecutableCommand
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    private readonly List<OperationProgress> _operationProgresses = new();
    private readonly BehaviorSubject<OperationProgress?> _currentOperationProgress = new(null);
    public IReadOnlyList<ItemToMove> ItemsToMove { get; }

    internal MoveCommand(
        IEnumerable<ItemToMove> itemsToMove,
        IContentAccessorFactory contentAccessorFactory,
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)
        : base("Move")
    {
        _contentAccessorFactory = contentAccessorFactory;
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        ItemsToMove = itemsToMove.ToList().AsReadOnly();
    }

    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO

        return Task.FromResult(CanCommandRun.True);
    }

    public override Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        //TODO

        return Task.FromResult(currentTime);
    }

    public async Task Execute()
    {
        Calculate();
        await Move();
    }

    private void Calculate()
    {
        _operationProgresses.Clear();
        _operationProgresses.AddRange(ItemsToMove.Select(i => new OperationProgress(i.Source.Path, 1)));

        //TODO: Handle IDisposable
        TrackProgress(_operationProgresses);
    }

    private async Task Move()
    {
        Dictionary<string, IItemMover> itemMovers = new();
        foreach (var itemToMove in ItemsToMove)
        {
            var currentOperationProgress =_operationProgresses.Find(p => p.Key == itemToMove.Source.Path)!;
            _currentOperationProgress.OnNext(currentOperationProgress);
            
            var sourceItem = await _timelessContentProvider.GetItemByFullNameAsync(itemToMove.Source, PointInTime.Present);

            var itemMover = GetOrAddItemMover(sourceItem.Provider);
            // Note: this is currently used for rename, so Target will be always the source container too
            // If this will be used for move between different containers, ContentProvider should be checked
            // And it should fall back to a copy+delete if the content providers are different
            await itemMover.RenameAsync(sourceItem.Provider, itemToMove.Source, itemToMove.Target);

            if (itemToMove.Source.GetParent() is { } parent)
                await _commandSchedulerNotifier.RefreshContainer(parent);
            
            await currentOperationProgress.SetProgressAsync(1);
        }

        IItemMover GetOrAddItemMover(IContentProvider provider)
        {
            if (itemMovers.TryGetValue(provider.Name, out var mover))
            {
                return mover;
            }

            var itemMover = _contentAccessorFactory.GetItemMover(provider);
            itemMovers.Add(provider.Name, itemMover);
            return itemMover;
        }
    }
}