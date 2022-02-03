using AsyncEvent;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public class DeleteCommand : IExecutableCommand
    {
        private Func<IContainer, Task>? _deleteContainer;
        private Func<IElement, Task>? _deleteElement;

        public int Progress => 100;

        public AsyncEventHandler ProgressChanged { get; } = new();

        public IList<AbsolutePath> ItemsToDelete { get; } = new List<AbsolutePath>();
        public string DisplayLabel { get; } = "DeleteCommand";

        public bool HardDelete { get; set; }
        public IReadOnlyList<string> CanRunMessages { get; } = new List<string>().AsReadOnly();

        public async Task<PointInTime> SimulateCommand(PointInTime startPoint)
        {
            var newDifferences = new List<Difference>();

            _deleteContainer = (c) =>
            {
                newDifferences.Add(new Difference(
                    DifferenceItemType.Container,
                    DifferenceActionType.Delete,
                    new AbsolutePath(c)
                ));

                return Task.CompletedTask;
            };
            _deleteElement = (e) =>
            {
                newDifferences.Add(new Difference(
                    DifferenceItemType.Element,
                    DifferenceActionType.Delete,
                    new AbsolutePath(e)
                ));

                return Task.CompletedTask;
            };

            foreach (var item in ItemsToDelete)
            {
                await TraverseTree((await item.Resolve())!);
            }

            return startPoint.WithDifferences(newDifferences);
        }

        public async Task Execute(TimeRunner timeRunner)
        {
            _deleteContainer = async (c) =>
            {
                await c.Delete(HardDelete);
                await timeRunner.RefreshContainer.InvokeAsync(this, new AbsolutePath(c));
            };
            _deleteElement = async (e) => await e.Delete(HardDelete);

            foreach (var item in ItemsToDelete)
            {
                await TraverseTree((await item.Resolve())!);
            }
        }

        private async Task TraverseTree(IItem item)
        {
            if (item is IContainer container)
            {
                if (!HardDelete && container.SupportsDirectoryLevelSoftDelete)
                {
                    if (_deleteContainer != null) await _deleteContainer.Invoke(container);
                }
                else
                {
                    foreach (var child in (await container.GetItems())!)
                    {
                        await TraverseTree(child);
                    }

                    if (_deleteContainer != null) await _deleteContainer.Invoke(container);
                }

            }
            else if (item is IElement element)
            {
                if (_deleteElement != null) await _deleteElement.Invoke(element);
            }
        }

        public async Task<CanCommandRun> CanRun(PointInTime startPoint)
        {
            var result = CanCommandRun.True;
            foreach (var itemPath in ItemsToDelete)
            {
                var resolvedItem = await itemPath.Resolve();
                if (resolvedItem != null
                    && (
                        resolvedItem.CanDelete == SupportsDelete.False
                        || (resolvedItem.CanDelete == SupportsDelete.HardDeleteOnly && !HardDelete)
                    )
                )
                {
                    result = CanCommandRun.Forceable;
                }
            }

            return result;
        }
    }
}