using FileTime.Core.Enums;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CopyCommand : ITransportationCommand
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandSchedulerNotifier _commandSchedulerNotifier;

    private readonly List<OperationProgress> _operationProgresses = new();

    public IList<FullName> Sources { get; } = new List<FullName>();

    public FullName? Target { get; set; }

    public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;
    public OperationProgress? CurrentOperationProgress { get; private set; }

    public CopyCommand(
        ITimelessContentProvider timelessContentProvider,
        ICommandSchedulerNotifier commandSchedulerNotifier)
    {
        _timelessContentProvider = timelessContentProvider;
        _commandSchedulerNotifier = commandSchedulerNotifier;
    }

    public Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        //TODO: 
        return Task.FromResult(CanCommandRun.True);
    }

    public async Task<PointInTime> SimulateCommand(PointInTime currentTime)
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
                if (!((await resolvedTarget.Items.GetItemsAsync())?.Any(i => i.Path.GetName() == item.Name) ?? false))
                {
                    await copyOperation.CreateContainerAsync(resolvedTarget, container.Name, container.PointInTime);
                }

                var children = await container.Items.GetItemsAsync();
                if (children is null) continue;

                await TraverseTree(currentTime, children.Select(c => c.Path).ToList(), target.GetChild(item.Name, AbsolutePathType.Container), transportMode, copyOperation);
                await copyOperation.ContainerCopyDoneAsync(new AbsolutePath(_timelessContentProvider, container));
            }
            else if (item is IElement element)
            {
                var newElementName = await Helper.GetNewNameAsync(resolvedTarget, element.Name, transportMode);
                if (newElementName == null) continue;

                var newElementPath = target.GetChild(newElementName, AbsolutePathType.Element);

                var currentProgress = _operationProgresses.Find(o => o.Key == element.FullName!.Path);
                CurrentOperationProgress = currentProgress;

                await copyOperation.CopyAsync(new AbsolutePath(_timelessContentProvider, element), newElementPath, new CopyCommandContext(UpdateProgress, currentProgress));
            }
        }
    }

    private Task UpdateProgress()
    {
        //TODO
        return Task.CompletedTask;
    }
}