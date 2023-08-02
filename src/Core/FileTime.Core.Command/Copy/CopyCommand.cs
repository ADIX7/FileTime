using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using DeclarativeProperty;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Command.Copy;

public class CopyCommand : CommandBase, ITransportationCommand
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    private readonly ILogger<CopyCommand> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly List<OperationProgress> _operationProgresses = new();
    private readonly BehaviorSubject<OperationProgress?> _currentOperationProgress = new(null);

    private long _recentTotalSum;
    private readonly DeclarativeProperty<long> _recentTotalProcessed = new();
    private readonly DeclarativeProperty<DateTime> _recentStartTime = new();

    public IReadOnlyList<FullName> Sources { get; }

    public FullName Target { get; }

    public TransportMode TransportMode { get; }

    internal CopyCommand(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        ILogger<CopyCommand> logger,
        IReadOnlyCollection<FullName> sources,
        TransportMode mode,
        FullName targetFullName)
        : base("Copy - Calculating...")
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(targetFullName);
        
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
        _logger = logger;
        _currentOperationProgress
            .Select(p =>
            {
                if (p is null) return Observable.Never<int>();
                return p.Progress.Select(currentProgress =>
                    p.TotalCount == 0
                        ? 0
                        : (int) (currentProgress * 100 / p.TotalCount)
                );
            })
            .Switch()
            .Subscribe(SetCurrentProgress);

        Sources = new List<FullName>(sources).AsReadOnly();
        TransportMode = mode;
        Target = targetFullName;

        var recentSpeed = DeclarativePropertyHelpers.CombineLatest(
            _recentTotalProcessed,
            _recentStartTime,
            (total, start) =>
            {
                var elapsed = DateTime.Now - start;

                var size = new ByteSize(total / elapsed.TotalSeconds);
                return Task.FromResult(size + "/s");
            });

        recentSpeed
            .Debounce(TimeSpan.FromMilliseconds(500))
            .Subscribe(SetDisplayDetailLabel);
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

    public override void Cancel()
        => _cancellationTokenSource.Cancel();

    public async Task ExecuteAsync(CopyFunc copy)
    {
        var currentTime = PointInTime.Present;

        await CalculateProgressAsync(currentTime);

        var copyOperation = new CopyStrategy(copy, new CopyStrategyParam(_operationProgresses, _commandSchedulerNotifier.RefreshContainer));

        var resolvedTarget = await _timelessContentProvider.GetItemByFullNameAsync(Target, currentTime);

        _recentTotalSum = 0;
        await _recentTotalProcessed.SetValue(0);
        await _recentStartTime.SetValue(DateTime.Now);

        await TraverseTree(
            currentTime,
            Sources,
            new AbsolutePath(_timelessContentProvider, resolvedTarget),
            TransportMode,
            copyOperation);
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
                    if (done > statuses.Count) done = statuses.Count;

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
            if (_cancellationTokenSource.IsCancellationRequested) return;

            var resolvedTarget = (IContainer) await target.ResolveAsync() ?? throw new Exception();
            var item = await _timelessContentProvider.GetItemByFullNameAsync(source, currentTime);

            if (item is IContainer container)
            {
                if (resolvedTarget.Items.All(i => i.Path.GetName() != item.Name))
                {
                    await copyOperation.CreateContainerAsync(resolvedTarget, container.Name, container.PointInTime);
                }

                var children = container.Items;

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

                await copyOperation.CopyAsync(new AbsolutePath(_timelessContentProvider, element), newElementPath, new CopyCommandContext(UpdateProgress, currentProgress, _cancellationTokenSource.Token));
            }
        }
    }

    private readonly object _updateProgressLock = new();

    private Task UpdateProgress()
    {
        lock (_updateProgressLock)
        {
            var now = DateTime.Now;
            var delta = now - _recentStartTime.Value;
            if (delta.TotalSeconds > 5)
            {
                _recentTotalSum += _recentTotalProcessed.Value;
                _recentStartTime.SetValueSafe(now);
            }

            var totalProcessedBytes = _operationProgresses.Select(o => o.Progress.Value).Sum();
            _recentTotalProcessed.SetValueSafe(totalProcessedBytes - _recentTotalSum);
        }

        //Not used, progress is reactive in this command
        //Note: Maybe this should be removed altogether, and every command should use reactive progress
        return Task.CompletedTask;
    }
}