namespace FileTime.Tools.Extensions;

public static class ObjectExtensions
{
    public static TResult Map<T, TResult>(this T obj, Func<T, TResult> mapper)
        => mapper(obj);
    public static TResult? MapNull<T, TResult>(this T obj, Func<TResult?> nullHandler, Func<T, TResult?> valueHandler)
        => obj == null ? nullHandler() : valueHandler(obj);

    public static TResult? MapNull<T, TResult>(this T obj, Func<T, TResult?> valueHandler)
        => obj == null ? default : valueHandler(obj);
}