namespace FileTime.Core.Extensions;

public static class TaskExtensions
{
    public static async Task TimeoutAfter(this Task task, int timeoutInMilliseconds)
    {
        if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)) != task) 
            throw new TimeoutException();
    }
    public static async Task<T?> TimeoutAfter<T>(this Task<T> task, int timeoutInMilliseconds, T? defaultValue = default)
    {
        if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)) == task)
        {
            return await task;
        }
        else
        {
            throw new TimeoutException();
        }
    }
}