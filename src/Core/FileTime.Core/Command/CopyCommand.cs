using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CopyCommand : ITransportationCommand
    {
        private Func<AbsolutePath, AbsolutePath, Task>? _copyOperation;
        private Dictionary<AbsolutePath, OperationProgress> _operationStatuses = new();
        private Func<IContainer, string, Task<IContainer>>? _createContainer;
        private Func<AbsolutePath, Task>? _containerCopyDone;

        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();

        public IContainer? Target { get; set; }

        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;

        public int Progress { get; private set; }

        public AsyncEventHandler ProgressChanged { get; } = new();

        public string DisplayLabel { get; } = "Copy";
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        private async Task UpdateProgress()
        {
            var total = 0;
            var current = 0;

            foreach (var item in _operationStatuses.Values)
            {
                current += item.Progress;
                total += item.TotalCount;
            }

            Progress = current * 100 / total;
            await ProgressChanged.InvokeAsync(this, AsyncEventArgs.Empty);
        }

        public async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            var newDiffs = new List<Difference>();

            _copyOperation = (_, to) =>
            {
                var target = to.GetParentAsAbsolutePath().Resolve();
                newDiffs.Add(new Difference(
                    target is IElement
                        ? DifferenceItemType.Element
                        : DifferenceItemType.Container,
                    DifferenceActionType.Create,
                    to
                ));

                return Task.CompletedTask;
            };

            _createContainer = async (IContainer target, string name) =>
            {
                var newContainerDiff = new Difference(
                    DifferenceItemType.Container,
                    DifferenceActionType.Create,
                    AbsolutePath.FromParentAndChildName(target, name)
                );

                newDiffs.Add(newContainerDiff);

                return (IContainer)(await newContainerDiff.AbsolutePath.Resolve())!;
            };

            await TraverseTree(Sources, Target, TransportMode.Value);

            return startPoint.WithDifferences(newDiffs);
        }

        public async Task Execute(Action<AbsolutePath, AbsolutePath> copy, TimeRunner timeRunner)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            await CalculateProgress();

            _copyOperation = async (from, to) =>
            {
                copy(from, to);
                var parentPath = to.GetParentAsAbsolutePath();
                if (_operationStatuses.ContainsKey(parentPath))
                {
                    _operationStatuses[parentPath].Progress++;
                }
                await UpdateProgress();
            };

            _createContainer = async (IContainer target, string name) => await target.CreateContainer(name);
            _containerCopyDone = async (path) =>
            {
                _operationStatuses[path].Progress = _operationStatuses[path].TotalCount;
                if (timeRunner != null)
                {
                    await timeRunner.RefreshContainer.InvokeAsync(this, path);
                }
            };

            await TraverseTree(Sources, Target, TransportMode.Value);
        }

        private async Task CalculateProgress()
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            var operationStatuses = new Dictionary<AbsolutePath, OperationProgress>();

            _copyOperation = (_, to) =>
            {
                var parentPath = to.GetParentAsAbsolutePath();
                OperationProgress operation;
                if (operationStatuses.ContainsKey(parentPath))
                {
                    operation = operationStatuses[parentPath];
                }
                else
                {
                    operation = new OperationProgress();
                    operationStatuses.Add(parentPath, operation);
                }
                operation.TotalCount++;

                return Task.CompletedTask;
            };

            await TraverseTree(Sources, Target, TransportMode.Value);
            _operationStatuses = operationStatuses;
        }

        private async Task TraverseTree(
            IEnumerable<AbsolutePath> sources,
            IContainer target,
            TransportMode transportMode)
        {
            if (_copyOperation == null) throw new ArgumentException("No copy operation were given.");
            if (_createContainer == null) throw new ArgumentException("No container creation function were given.");

            foreach (var source in sources)
            {
                var item = await source.Resolve();

                if (item is IContainer container)
                {
                    var targetContainer = (await target.GetContainers())?.FirstOrDefault(d => d.Name == container.Name) ?? (await _createContainer?.Invoke(target, container.Name)!);

                    var childDirectories = (await container.GetContainers())!.Select(d => new AbsolutePath(d));
                    var childFiles = (await container.GetElements())!.Select(f => new AbsolutePath(f));

                    await TraverseTree(childDirectories.Concat(childFiles), targetContainer, transportMode);
                    if (_containerCopyDone != null) await _containerCopyDone.Invoke(new AbsolutePath(container));
                }
                else if (item is IElement element)
                {
                    var targetName = element.Name;

                    var targetNameExists = await target.IsExists(targetName);
                    if (transportMode == Command.TransportMode.Merge)
                    {
                        for (var i = 0; targetNameExists; i++)
                        {
                            targetName = element.Name + (i == 0 ? "_" : $"_{i}");
                        }
                    }
                    else if (transportMode == Command.TransportMode.Skip && targetNameExists)
                    {
                        continue;
                    }

                    _copyOperation?.Invoke(new AbsolutePath(element), AbsolutePath.FromParentAndChildName(target, targetName));
                }
            }
        }

        public Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            //TODO: implement
            return Task.FromResult(CanCommandRun.True);
        }
    }
}