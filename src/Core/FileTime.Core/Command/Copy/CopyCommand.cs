using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy
{
    public class CopyCommand : CommandBase, ITransportationCommand
    {
        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();

        public AbsolutePath? Target { get; set; }

        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;
        internal TimeRunner? TimeRunner { get; private set; }

        public bool TargetIsContainer => true;
        public List<InputElement> Inputs { get; } = new();
        public List<object>? InputResults { get; set; }

        public CopyCommand()
        {
            DisplayLabel = "Copy";
        }

        public override async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            var simulateOperation = new SimulateOperation();

            await TraverseTree(Sources, Target, TransportMode.Value, simulateOperation);

            return startPoint.WithDifferences(simulateOperation.NewDiffs);
        }

        public async Task Execute(Func<AbsolutePath, AbsolutePath, OperationProgress?, CopyCommandContext, Task> copy, TimeRunner timeRunner)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            TimeRunner = timeRunner;

            await CalculateProgress();

            var copyOperation = new CopyOperation(copy, this);

            await TraverseTree(Sources, Target, TransportMode.Value, copyOperation);
            await TimeRunner.RefreshContainer.InvokeAsync(this, Target);
        }

        private async Task CalculateProgress()
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            var calculateOperation = new CalculateOperation();
            await TraverseTree(Sources, Target, TransportMode.Value, calculateOperation);
            OperationStatuses = new Dictionary<AbsolutePath, List<OperationProgress>>(calculateOperation.OperationStatuses);
        }

        private async Task TraverseTree(
            IEnumerable<AbsolutePath> sources,
            AbsolutePath target,
            TransportMode transportMode,
            ICopyOperation copyOperation)
        {
            var resolvedTarget = (IContainer?)await target.ResolveAsync();

            foreach (var source in sources)
            {
                var item = await source.ResolveAsync();

                if (item is IContainer container)
                {
                    var targetContainer = target.GetChild(item.Name, AbsolutePathType.Container);
                    if (resolvedTarget != null)
                    {
                        await resolvedTarget.RefreshAsync();

                        if (!await resolvedTarget.IsExistsAsync(item.Name))
                        {
                            await copyOperation.CreateContainerAsync(resolvedTarget, container.Name);
                        }
                    }

                    await TraverseTree((await container.GetItems())!.Select(i => new AbsolutePath(i)), targetContainer, transportMode, copyOperation);
                    await copyOperation.ContainerCopyDoneAsync(new AbsolutePath(container));
                }
                else if (item is IElement element)
                {
                    var newElementName = await Helper.CommandHelper.GetNewNameAsync(resolvedTarget, element.Name, transportMode);
                    if (newElementName == null) continue;

                    OperationProgress? operation = null;
                    var newElementPath = target.GetChild(newElementName, AbsolutePathType.Element);

                    if (OperationStatuses.TryGetValue(target, out var targetPathOperations))
                    {
                        var path = new AbsolutePath(element).Path;
                        operation = targetPathOperations.Find(o => o.Key == path);
                    }
                    CurrentOperationProgress = operation;

                    await copyOperation.CopyAsync(new AbsolutePath(element), newElementPath, operation, new CopyCommandContext(UpdateProgress));
                }
            }
        }

        public override Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            //TODO: implement
            return Task.FromResult(CanCommandRun.True);
        }
    }
}