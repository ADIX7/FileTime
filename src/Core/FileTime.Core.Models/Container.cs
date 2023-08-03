using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models;

public record Container(
    string Name,
    string DisplayName,
    FullName FullName,
    NativePath NativePath,
    AbsolutePath? Parent,
    bool IsHidden,
    bool IsExists,
    DateTime? CreatedAt,
    DateTime? ModifiedAt,
    SupportsDelete CanDelete,
    bool CanRename,
    string? Attributes,
    IContentProvider Provider,
    bool AllowRecursiveDeletion,
    PointInTime PointInTime,
    ObservableCollection<Exception> Exceptions,
    ReadOnlyExtensionCollection Extensions,
    ObservableCollection<AbsolutePath> Items) : IContainer
{
    private readonly CancellationTokenSource _loadingCancellationTokenSource = new();
    private readonly BehaviorSubject<bool> _isLoading = new(false);

    public CancellationToken LoadingCancellationToken => _loadingCancellationTokenSource.Token;
    public IObservable<bool> IsLoading => _isLoading.AsObservable();
    public bool? IsLoaded { get; private set; }
    public AbsolutePathType Type => AbsolutePathType.Container;

    public async Task WaitForLoaded(CancellationToken token = default)
    {
        while (IsLoaded != true) await Task.Delay(1, token);
    }

    public void StartLoading()
    {
        _isLoading.OnNext(true);
        IsLoaded = false;
    }

    public void StopLoading()
    {
        _isLoading.OnNext(false);
        IsLoaded = true;
    }

    public void CancelLoading()
    {
        _loadingCancellationTokenSource.Cancel();
        _isLoading.OnNext(false);
        IsLoaded = true;
    }

    public IItem WithParent(AbsolutePath parent) => this with {Parent = parent};
}