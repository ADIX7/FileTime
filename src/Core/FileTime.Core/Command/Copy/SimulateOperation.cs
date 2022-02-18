using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy
{
    public class SimulateOperation : ICopyOperation
    {
        private readonly List<Difference> _newDiffs;
        public IReadOnlyList<Difference> NewDiffs { get; }

        public SimulateOperation()
        {
            _newDiffs = new List<Difference>();
            NewDiffs = _newDiffs.AsReadOnly();
        }
        public Task ContainerCopyDoneAsync(AbsolutePath path)
        {
            return Task.CompletedTask;
        }

        public Task CopyAsync(AbsolutePath from, AbsolutePath to, OperationProgress? operation, CopyCommandContext context)
        {
            _newDiffs.Add(new Difference(DifferenceActionType.Create, to));
            return Task.CompletedTask;
        }

        public Task CreateContainerAsync(IContainer target, string name)
        {
            var newContainerDiff = new Difference(
                DifferenceActionType.Create,
                AbsolutePath.FromParentAndChildName(target, name, AbsolutePathType.Container)
            );

            _newDiffs.Add(newContainerDiff);

            return Task.FromResult((IContainer)null!);
        }
    }
}