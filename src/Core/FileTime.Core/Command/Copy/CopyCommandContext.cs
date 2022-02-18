namespace FileTime.Core.Command.Copy
{
    public class CopyCommandContext
    {
        private readonly Func<Task> _updateProgress;

        public CopyCommandContext(Func<Task> updateProgress)
        {
            _updateProgress = updateProgress;
        }

        public async Task UpdateProgress() => await _updateProgress.Invoke();
    }
}