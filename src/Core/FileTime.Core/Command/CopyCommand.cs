using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CopyCommand : ITransportationCommand
    {
        private Action<AbsolutePath, AbsolutePath>? _copyOperation;
        private Func<IContainer, string, Task<IContainer>>? _createContainer;
        private TimeRunner? _timeRunner;

        public IList<AbsolutePath>? Sources { get; } = new List<AbsolutePath>();

        public IContainer? Target { get; set; }

        public TransportMode? TransportMode { get; set; } = Command.TransportMode.Merge;

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

            await DoCopy(Sources, Target, TransportMode.Value);

            return startPoint.WithDifferences(newDiffs);
        }

        public async Task Execute(Action<AbsolutePath, AbsolutePath> copy, TimeRunner timeRunner)
        {
            if (Sources == null) throw new ArgumentException(nameof(Sources) + " can not be null");
            if (Target == null) throw new ArgumentException(nameof(Target) + " can not be null");
            if (TransportMode == null) throw new ArgumentException(nameof(TransportMode) + " can not be null");

            _copyOperation = copy;
            _createContainer = async (IContainer target, string name) => await target.CreateContainer(name);
            _timeRunner = timeRunner;

            await DoCopy(Sources, Target, TransportMode.Value);
        }

        private async Task DoCopy(
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

                    await DoCopy(childDirectories.Concat(childFiles), targetContainer, transportMode);
                    _timeRunner?.RefreshContainer.InvokeAsync(this, new AbsolutePath(container));
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