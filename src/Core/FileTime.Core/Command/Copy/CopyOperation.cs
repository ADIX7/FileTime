using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy
{
    public class CopyOperation : ICopyOperation
    {
        private readonly Func<AbsolutePath, AbsolutePath, OperationProgress?, CopyCommandContext, Task> _copy;
        private readonly CopyCommand _copyCommand;

        public CopyOperation(Func<AbsolutePath, AbsolutePath, OperationProgress?, CopyCommandContext, Task> copy, CopyCommand copyCommand)
        {
            _copy = copy;
            _copyCommand = copyCommand;
        }

        public async Task CopyAsync(AbsolutePath from, AbsolutePath to, OperationProgress? operation, CopyCommandContext context)
        {
            await _copy(from, to, operation, context);
            if (operation != null)
            {
                operation.Progress = operation.TotalCount;
            }
            await _copyCommand.UpdateProgress();
        }

        public async Task CreateContainerAsync(IContainer target, string name) => await target.CreateContainerAsync(name);

        public async Task ContainerCopyDoneAsync(AbsolutePath path)
        {
            if (_copyCommand.OperationStatuses.ContainsKey(path))
            {
                foreach (var item in _copyCommand.OperationStatuses[path])
                {
                    item.Progress = item.TotalCount;
                }
            }

            if (_copyCommand.TimeRunner != null)
            {
                await _copyCommand.TimeRunner.RefreshContainer.InvokeAsync(this, path);
            }
        }
    }
}