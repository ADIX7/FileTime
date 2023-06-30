using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CopyCommand : CommandBase, ITransportationCommand
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    private readonly List<OperationProgress> _operationProgresses = new();
    private readonly BehaviorSubject<OperationProgress?> _currentOperationProgress = new(null);

    public IList<FullName> Sources { get; } = new List<FullName>();

    public FullName? Target { get; set; }

    public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;
    public IObservable<OperationProgress?> CurrentOperationProgress { get; }

    public CopyCommand(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)
        : base("Copy")
    {
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        CurrentOperationProgress = _currentOperationProgress.AsObservable();
    }

    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(CanCommandRun.True);
    }

    public override async Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
        if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
        if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

        var simulateOperation = new SimulateStrategy(_timelessContentProvider);
        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode.Value,
            simulateOperation);

        return currentTime.WithDifferences(simulateOperation.NewDiffs);
    }

    public async Task ExecuteAsync(CopyFunc copy)
    {
        if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
        if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
        if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

        var currentTime = PointInTime.Present;

        await CalculateProgressAsync(currentTime);

        var copyOperation = new CopyStrategy(copy, new CopyStrategyParam(_operationProgresses, _commandSchedulerNotifier.RefreshContainer));

        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode.Value,
            copyOperation);
        //await TimeRunner.RefreshContainer.InvokeAsync(this, Target);
    }

    private async Task CalculateProgressAsync(PointInTime currentTime)
    {
        if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
        if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
        if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

        var calculateOperation = new CalculateStrategy();
        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode.Value,
            calculateOperation);

        _operationProgresses.Clear();
        _operationProgresses.AddRange(calculateOperation.OperationStatuses);

        _operationProgresses
            .Select(op => op.Progress.Select(p => (Progress: p, TotalProgress: op.TotalCount)))
            .CombineLatest()
            .Select(data =>
            {
                var total = data.Sum(d => d.TotalProgress);
                if (total == 0) return 0;
                return (int) (data.Sum(d => d.Progress) * 100 / total);
            })
            .Subscribe(SetTotalProgress);
    }

    private async Task TraverseTree(
        PointInTime currentTime,
        IEnumerable<FullName> sources,
        AbsolutePath target,
        TransportMode transportMode,
        ICopyStrategy copyOperation)
    {
        var resolvedTarget = ((IContainer) await target.ResolveAsync()) ?? throw new Exception();

        foreach (var source in sources)
        {
            var item = await _timelessContentProvider.GetItemByFullNameAsync(source, currentTime);

            if (item is IContainer container)
            {
                if (resolvedTarget.ItemsCollection.All(i => i.Path.GetName() != item.Name))
                {
                    await copyOperation.CreateContainerAsync(resolvedTarget, container.Name, container.PointInTime);
                }

                var children = container.ItemsCollection;

                await TraverseTree(currentTime, children.Select(c => c.Path).ToList(), target.GetChild(item.Name, AbsolutePathType.Container), transportMode, copyOperation);
                await copyOperation.ContainerCopyDoneAsync(new AbsolutePath(_timelessContentProvider, container));
            }
            else if (item is IElement element)
            {
                var newElementName = await Helper.GetNewNameAsync(resolvedTarget, element.Name, transportMode);
                if (newElementName == null) continue;

                var newElementPath = target.GetChild(newElementName, AbsolutePathType.Element);

                var currentProgress = _operationProgresses.Find(o => o.Key == element.FullName!.Path);
                _currentOperationProgress.OnNext(currentProgress);

                await copyOperation.CopyAsync(new AbsolutePath(_timelessContentProvider, element), newElementPath, new CopyCommandContext(UpdateProgress, currentProgress));
            }
        }
    }

    private Task UpdateProgress() =>
        //Not used, progress is reactive in this command
        //Note: Maybe this should be removed altogether, and every command should use reactive progress
        Task.CompletedTask;
}