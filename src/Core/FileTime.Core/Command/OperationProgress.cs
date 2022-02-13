namespace FileTime.Core.Command
{
    public class OperationProgress
    {
        public string Key { get; }
        public long Progress { get; set; }
        public long TotalCount { get; }
        public bool IsDone => Progress == TotalCount;

        public OperationProgress(string key, long totalCount)
        {
            Key = key;
            TotalCount = totalCount;
        }
    }
}