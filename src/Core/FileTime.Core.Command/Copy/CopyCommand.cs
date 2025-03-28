using DeclarativeProperty;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Command.Copy;

public class CopyCommand : CommandBase, ITransportationCommand
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;
    private readonly CopyStrategyFactory _copyStrategyFactory;
    private readonly ILogger<CopyCommand> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly List<OperationProgress> _operationProgresses = new();
    private readonly DeclarativeProperty<OperationProgress?> _currentOperationProgress = new(null);

    private long _recentTotalSum;
    private readonly DeclarativeProperty<long> _recentTotalProcessed = new(0);
    private readonly DeclarativeProperty<DateTime> _recentStartTime = new(DateTime.Now);

    public IReadOnlyList<FullName> Sources { get; }

    public FullName Target { get; }

    public TransportMode TransportMode { get; }

    internal CopyCommand(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier,
        CopyStrategyFactory copyStrategyFactory,
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
        _copyStrategyFactory = copyStrategyFactory;
        _logger = logger;
        _currentOperationProgress
            .Map(p =>
            {
                return p?.Progress.Map(currentProgress =>
                    p.TotalCount == 0
                        ? 0
                        : (int)(currentProgress * 100 / p.TotalCount)
                );
            })
            .Switch()
            .Subscribe(async (p, _) => await SetCurrentProgress(p));

        Sources = new List<FullName>(sources).AsReadOnly();
        TransportMode = mode;
        Target = targetFullName;

        var recentSpeed = DeclarativePropertyHelpers.CombineLatest(
            _recentTotalProcessed,
            _recentStartTime,
            (total, start) =>
            {
                var elapsed = DateTime.Now - start;

                var size = ByteSize.FromBytes(total / elapsed.TotalSeconds);
                return Task.FromResult(size + "/s");
            });

        recentSpeed
            .Debounce(TimeSpan.FromMilliseconds(500))
            .Subscribe(async (l, _) => await SetDisplayDetailLabel(l));
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

        var copyOperation = _copyStrategyFactory.CreateCopyStrategy(copy, _operationProgresses, _commandSchedulerNotifier.RefreshContainer);

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
            await SetDisplayLabelAsync($"Copy - {Sources[0].GetName()}");
        }
        else
        {
            _operationProgresses
                .Select(o => o.IsDone)
                .CombineAll(statuses =>
                {
                    var statusList = statuses.ToList();
                    var done = statusList.Count(s => s) + 1;
                    if (done > statusList.Count) done = statusList.Count;

                    return Task.FromResult($"Copy - {done} / {statusList.Count}");
                })
                .Subscribe(async (v, _) => await SetDisplayLabelAsync(v));
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

            var resolvedTarget = (IContainer)await target.ResolveAsync() ?? throw new Exception();
            var item = await _timelessContentProvider.GetItemByFullNameAsync(source, currentTime);

            if (item is IContainer container)
            {
                try
                {
                    await container.WaitForLoaded(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

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
                await _currentOperationProgress.SetValue(currentProgress);

                try
                {
                    await copyOperation.CopyAsync(new AbsolutePath(_timelessContentProvider, element), newElementPath, new CopyCommandContext(UpdateProgress, currentProgress, _cancellationTokenSource.Token));
                }
                catch (Exception e)
                {
                    _logger.LogError("Error while copying file: {Path}, {Message}", element.FullName!.Path, e.Message);
                    AddError(new CommandError("Error while copying file: " + element.FullName!.Path, e));
                }
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
