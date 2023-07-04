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

    public IReadOnlyList<FullName> Sources { get; }

    public FullName Target { get; }

    public TransportMode TransportMode { get; }

    public CopyCommand(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        IReadOnlyCollection<FullName>? sources,
        TransportMode? mode,
        FullName? targetFullName)
        : base("Copy - Calculating...")
    {
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        _currentOperationProgress
            .Select(p =>
            {
                if (p is null) return Observable.Never<int>();
                return p.Progress.Select(currentProgress => (int)(currentProgress * 100 / p.TotalCount));
            })
            .Switch()
            .Subscribe(SetCurrentProgress);

        if (sources is null) throw new ArgumentException(nameof(Sources) + " can not be null");
        if (targetFullName is null) throw new ArgumentException(nameof(Target) + " can not be null");
        if (mode is null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

        Sources = new List<FullName>(sources).AsReadOnly();
        TransportMode = mode.Value;
        Target = targetFullName;
    }

    public override Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(CanCommandRun.True);
    }

    public override async Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        var simulateOperation = new SimulateStrategy(_timelessContentProvider);
        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode,
            simulateOperation);

        return currentTime.WithDifferences(simulateOperation.NewDiffs);
    }

    public async Task ExecuteAsync(CopyFunc copy)
    {
        var currentTime = PointInTime.Present;

        await CalculateProgressAsync(currentTime);

        var copyOperation = new CopyStrategy(copy, new CopyStrategyParam(_operationProgresses, _commandSchedulerNotifier.RefreshContainer));

        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode,
            copyOperation);
        //await TimeRunner.RefreshContainer.InvokeAsync(this, Target);
    }

    private async Task CalculateProgressAsync(PointInTime currentTime)
    {
        var calculateOperation = new CalculateStrategy();
        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode,
            calculateOperation);

        _operationProgresses.Clear();
        _operationProgresses.AddRange(calculateOperation.OperationStatuses);

        //TODO: Handle IDisposable
        TrackProgress(_operationProgresses);


        if (Sources.Count == 1)
        {
            SetDisplayLabel($"Copy - {Sources[0].GetName()}");
        }
        else
        {
            _operationProgresses
                .Select(o => o.IsDone)
                .CombineLatest()
                .Subscribe(statuses =>
                {
                    var done = statuses.Count(s => s) + 1;
                    if(done > statuses.Count) done = statuses.Count;

                    SetDisplayLabel($"Copy - {done} / {statuses.Count}");
                });
        }
    }

    private async Task TraverseTree(
        PointInTime currentTime,
        IEnumerable<FullName> sources,
        AbsolutePath target,
        TransportMode transportMode,
        ICopyStrategy copyOperation)
    {
        foreach (var source in sources)
        {
            var resolvedTarget = (IContainer)await target.ResolveAsync() ?? throw new Exception();
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