using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CopyCommand : ITransportationCommand
    {
        private Func<AbsolutePath, AbsolutePath, OperationProgress?, CopyCommandContext, Task>? _copyOperation;
        private Dictionary<AbsolutePath, List<OperationProgress>> _operationStatuses = new();
        private Func<IContainer, string, Task<IContainer>>? _createContainer;
        private Func<AbsolutePath, Task>? _containerCopyDone;
        private OperationProgress? _currentOperationProgress;

        public IList<AbsolutePath> Sources { get; } = new List<AbsolutePath>();

        public IContainer? Target { get; set; }

        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;

        public int Progress { get; private set; }
        public int CurrentProgress { get; private set; }

        public AsyncEventHandler ProgressChanged { get; } = new();

        public string DisplayLabel { get; } = "Copy";
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        private async Task UpdateProgress()
        {
            var total = 0L;
            var current = 0L;

            foreach (var folder in _operationStatuses.Values)
            {
                foreach (var item in folder)
                {
                    current += item.Progress;
                    total += item.TotalCount;
                }
            }

            Progress = (int)(current * 100 / total);
            if (_currentOperationProgress == null)
            {
                CurrentProgress = 0;
            }
            else
            {
                CurrentProgress = (int)(_currentOperationProgress.Progress * 100 / _currentOperationProgress.TotalCount);
            }
            await ProgressChanged.InvokeAsync(this, AsyncEventArgs.Empty);
        }

        public async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            var newDiffs = new List<Difference>();

            _copyOperation = (_, to, _, _) =>
            {
                var target = to.GetParent().ResolveAsync();
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
                    AbsolutePath.FromParentAndChildName(target, name, AbsolutePathType.Container)
                );

                newDiffs.Add(newContainerDiff);

                return (IContainer)(await newContainerDiff.AbsolutePath.ResolveAsync())!;
            };

            await TraverseTree(Sources, Target, TransportMode.Value);

            return startPoint.WithDifferences(newDiffs);
        }

        public async Task Execute(Func<AbsolutePath, AbsolutePath, OperationProgress?, CopyCommandContext, Task> copy, TimeRunner timeRunner)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            await CalculateProgress();

            _copyOperation = async (from, to, operation, context) =>
            {
                await copy(from, to, operation, context);
                if (operation != null)
                {
                    operation.Progress = operation.TotalCount;
                }
                await UpdateProgress();
            };

            _createContainer = async (IContainer target, string name) => await target.CreateContainerAsync(name);
            _containerCopyDone = async (path) =>
            {
                foreach (var item in _operationStatuses[path])
                {
                    item.Progress = item.TotalCount;
                }

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

            var operationStatuses = new Dictionary<AbsolutePath, List<OperationProgress>>();

            _copyOperation = async (from, to, _, _) =>
            {
                var parentPath = to.GetParent();
                List<OperationProgress> operationsByFolder;
                if (operationStatuses.ContainsKey(parentPath))
                {
                    operationsByFolder = operationStatuses[parentPath];
                }
                else
                {
                    var resolvedFrom = await from.ResolveAsync();
                    operationsByFolder = new List<OperationProgress>();
                    operationStatuses.Add(parentPath, operationsByFolder);
                    operationsByFolder.Add(new OperationProgress(from.Path, resolvedFrom is IElement element ? await element.GetElementSize() : 0L));
                }
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
                var item = await source.ResolveAsync();

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

                    var targetNameExists = await target.IsExistsAsync(targetName);
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

                    OperationProgress? operation = null;
                    var targetFolderPath = new AbsolutePath(target);
                    var targetElementPath = AbsolutePath.FromParentAndChildName(target, targetName, AbsolutePathType.Element);

                    foreach(var asd in _operationStatuses.Keys)
                    {
                        var hash1 = asd.GetHashCode();
                        var hash2 = targetFolderPath.GetHashCode();
                        var eq = asd == targetFolderPath;
                    }

                    if (_operationStatuses.TryGetValue(targetFolderPath, out var targetPathOperations))
                    {
                        var path = new AbsolutePath(element).Path;
                        operation = targetPathOperations.Find(o => o.Key == path);
                    }
                    _currentOperationProgress = operation;

                    if (_copyOperation != null) await _copyOperation.Invoke(new AbsolutePath(element), targetElementPath, operation, new CopyCommandContext(UpdateProgress));
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