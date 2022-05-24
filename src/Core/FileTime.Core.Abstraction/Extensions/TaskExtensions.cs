namespace FileTime.Core.Extensions;

public static class TaskExtensions
{
    public static async Task<T?> AwaitWithTimeout<T>(this Task<T> task, int timeout, T? defaultValue = default)
    {
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        {
            return task.Result;
        }
        else
        {
            return defaultValue;
        }
    }
}