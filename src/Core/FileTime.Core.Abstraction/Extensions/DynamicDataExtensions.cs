using System.Reactive.Linq;
using DynamicData;
using FileTime.Core.Models;

namespace FileTime.Core.Extensions;

public static class DynamicDataExtensions
{
    private class DisposableContext<TParam, TTaskResult>
    {
        private readonly Func<TParam, TTaskResult> _transformResult;
        private readonly TaskCompletionSource<TTaskResult?> _taskCompletionSource;
        public IDisposable? Disposable { get; set; }

        public DisposableContext(Func<TParam, TTaskResult> transformResult,
            TaskCompletionSource<TTaskResult?> taskCompletionSource, IDisposable? disposable = null)
        {
            _transformResult = transformResult;
            _taskCompletionSource = taskCompletionSource;
            Disposable = disposable;
        }

        public void OnNext(TParam param)
        {
            Disposable?.Dispose();
            var result = _transformResult(param);
            _taskCompletionSource.SetResult(result);
        }

        public void OnError(Exception ex)
        {
            Disposable?.Dispose();
            _taskCompletionSource.SetException(ex);
        }

        public void OnCompleted()
        {
            Disposable?.Dispose();
            _taskCompletionSource.SetResult(default);
        }
    }

    public static async Task<IEnumerable<AbsolutePath>?> GetItemsAsync(
        this IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> stream)
        => await GetItemsAsync(stream
            .Select(s =>
                s is null
                    ? new SourceList<AbsolutePath>().Connect().StartWithEmpty().ToCollection()
                    : s.ToCollection())
            .Switch());

    public static async Task<IEnumerable<AbsolutePath>?> GetItemsAsync(
        this IObservable<IChangeSet<AbsolutePath, string>> stream)
        => await GetItemsAsync(stream.ToCollection());

    public static Task<IEnumerable<AbsolutePath>?> GetItemsAsync(
        this IObservable<IReadOnlyCollection<AbsolutePath>> stream)
    {
        var taskCompletionSource = new TaskCompletionSource<IEnumerable<AbsolutePath>?>();
        var context = new DisposableContext<IReadOnlyCollection<AbsolutePath>, IEnumerable<AbsolutePath>?>(r => r,
            taskCompletionSource);

        context.Disposable = stream
            .Subscribe(
                context.OnNext,
                context.OnError,
                context.OnCompleted
            );

        return taskCompletionSource.Task;
    }
}