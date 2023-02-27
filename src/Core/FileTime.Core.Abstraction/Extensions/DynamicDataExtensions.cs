using System.Reactive.Linq;
using DynamicData;
using FileTime.Core.Models;

namespace FileTime.Core.Extensions;

public static class DynamicDataExtensions
{
    private class DisposableContext<TParam, TTaskResult>
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Func<TParam, TTaskResult> _transformResult;
        private readonly TaskCompletionSource<TTaskResult?> _taskCompletionSource;
        private bool _isFinished;
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
            if (IsFinished()) return;
            Disposable?.Dispose();
            var result = _transformResult(param);
            _taskCompletionSource.SetResult(result);
        }

        public void OnError(Exception ex)
        {
            if (IsFinished()) return;
            Disposable?.Dispose();
            _taskCompletionSource.SetException(ex);
        }

        public void OnCompleted()
        {
            if (IsFinished()) return;
            Disposable?.Dispose();
            _taskCompletionSource.SetResult(default);
        }

        private bool IsFinished()
        {
            _semaphore.Wait();
            var finished = _isFinished;
            _isFinished = true;
            _semaphore.Release();

            return finished;
        }
    }

    public static async Task<IEnumerable<AbsolutePath>?> GetItemsAsync(
        this IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> stream)
        => await GetItemsAsync(stream
            .Select(s =>
                s is null
                    ? new SourceList<AbsolutePath>().Connect().StartWithEmpty().ToCollection()
                    : s.StartWithEmpty().ToCollection())
            .Switch());

    public static async Task<IEnumerable<AbsolutePath>?> GetItemsAsync(
        this IObservable<IChangeSet<AbsolutePath, string>> stream)
        => await GetItemsAsync(stream.StartWithEmpty().ToCollection());

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