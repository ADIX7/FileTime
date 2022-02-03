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
        public AsyncEventHandler ProgressChanged { get; } = new();
        public string DisplayLabel { get; } = "CreateElement";
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public CreateElementCommand(AbsolutePath container, string newElementName)
        {
            Container = container;
            NewElementName = newElementName;
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            var possibleContainer = await Container.Resolve();
            if (possibleContainer is IContainer container)
            {
                await container.CreateElement(NewElementName);
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(container));
            }
        }

        public Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>()
            {
                new Difference(DifferenceItemType.Element, DifferenceActionType.Create, new AbsolutePath(Container.ContentProvider, Container.Path + Constants.SeparatorChar + NewElementName, Container.VirtualContentProvider))
            };
            return Task.FromResult(startPoint.WithDifferences(newDifferences));
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            var resolvedContainer = Container.Resolve();
            if (resolvedContainer == null) return CanCommandRun.Forceable;

            if (resolvedContainer is not IContainer container
                || await container.IsExists(NewElementName))
            {
                return CanCommandRun.False;
            }

            return CanCommandRun.True;
        }
    }
}