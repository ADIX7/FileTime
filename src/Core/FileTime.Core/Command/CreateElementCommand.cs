using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class CreateElementCommand : IExecutableCommand
    {
        public AbsolutePath Container { get; }
        public string NewElementName { get; }

        public int Progress => 100;
        public int CurrentProgress => 100;
        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; }
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public CreateElementCommand(AbsolutePath container, string newElementName)
        {
            Container = container;
            NewElementName = newElementName;
            DisplayLabel = $"Create element {newElementName}";
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            var possibleContainer = await Container.ResolveAsync();
            if (possibleContainer is IContainer container)
            {
                await container.CreateElementAsync(NewElementName);
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(container));
            }
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>()
            {
                new Difference(DifferenceActionType.Create, Container.GetChild(NewElementName, AbsolutePathType.Element))
            };
            return Task.FromResult(startPoint.WithDifferences(newDifferences));
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            var resolvedContainer = await Container.ResolveAsync();
            if (resolvedContainer == null) return CanCommandRun.Forceable;

            if (resolvedContainer is not IContainer container
                || await container.IsExistsAsync(NewElementName))
            {
                return CanCommandRun.False;
            }

            return CanCommandRun.True;
        }
    }
}