using System.Collections.ObjectModel;
using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy
{
    public class CalculateOperation : ICopyOperation
    {
        private readonly Dictionary<AbsolutePath, List<OperationProgress>> _operationStatuses;
        public IReadOnlyDictionary<AbsolutePath, List<OperationProgress>> OperationStatuses { get; }

        public CalculateOperation()
        {
            _operationStatuses = new Dictionary<AbsolutePath, List<OperationProgress>>();
            OperationStatuses = new ReadOnlyDictionary<AbsolutePath, List<OperationProgress>>(_operationStatuses);
        }

        public Task ContainerCopyDoneAsync(AbsolutePath path)
        {
            return Task.CompletedTask;
        }

        public async Task CopyAsync(AbsolutePath from, AbsolutePath to, OperationProgress? operation, CopyCommandContext context)
        {
            var parentPath = to.GetParent();
            List<OperationProgress> operationsByFolder;
            if (_operationStatuses.ContainsKey(parentPath))
            {
                operationsByFolder = _operationStatuses[parentPath];
            }
            else
            {
                operationsByFolder = new List<OperationProgress>();
                _operationStatuses.Add(parentPath, operationsByFolder);
            }

            var resolvedFrom = await from.ResolveAsync();
            operationsByFolder.Add(new OperationProgress(from.Path, resolvedFrom is IElement element ? await element.GetElementSize() : 0L));
        }

        public Task CreateContainerAsync(IContainer target, string name)
        {
            return Task.CompletedTask;
        }
    }
}