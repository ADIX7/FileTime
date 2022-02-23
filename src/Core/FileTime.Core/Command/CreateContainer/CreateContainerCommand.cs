using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateContainer
{
    public class CreateContainerCommand : IExecutableCommand
    {
        public AbsolutePath Container { get; }
        public string NewContainerName { get; }

        public int Progress => 100;
        public int CurrentProgress => 100;

        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; }
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public CreateContainerCommand(AbsolutePath container, string newContainerName)
        {
            Container = container;
            NewContainerName = newContainerName;
            DisplayLabel = $"Create container {newContainerName}";
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            var possibleContainer = await Container.ResolveAsync();
            if (possibleContainer is IContainer container)
            {
                await container.CreateContainerAsync(NewContainerName);
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(container));
            }
            //TODO: else
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>()
            {
                new Difference(DifferenceActionType.Create, Container.GetChild(NewContainerName, AbsolutePathType.Container))
            };
            return Task.FromResult(startPoint.WithDifferences(newDifferences));
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            var resolvedContainer = await Container.ResolveAsync();
            if (resolvedContainer == null) return CanCommandRun.Forceable;

            if (resolvedContainer is not IContainer container
                || await container.IsExistsAsync(NewContainerName))
            {
                return CanCommandRun.False;
            }

            return CanCommandRun.True;
        }
    }
}