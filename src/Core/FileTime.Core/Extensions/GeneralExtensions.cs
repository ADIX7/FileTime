namespace FileTime.Core.Extensions
{
    public static class GeneralExtensions
    {
        public static TResult Map<T, TResult>(this T obj, Func<T, TResult> map) => map(obj);
    }
}