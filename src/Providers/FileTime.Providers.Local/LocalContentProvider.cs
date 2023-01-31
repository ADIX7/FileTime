using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.Core.Timeline;

namespace FileTime.Providers.Local;

public sealed partial class LocalContentProvider : ContentProviderBase, ILocalContentProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly SourceCache<AbsolutePath, string> _rootDirectories = new(i => i.Path.Path);
    private readonly bool _isCaseInsensitive;

    public LocalContentProvider(ITimelessContentProvider timelessContentProvider) : base("local")
    {
        _timelessContentProvider = timelessContentProvider;
        _isCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        SupportsContentStreams = true;

        RefreshRootDirectories();

        Items.OnNext(_rootDirectories.Connect());
    }

    public override Task OnEnter()
    {
        RefreshRootDirectories();

        return Task.CompletedTask;
    }

    private void RefreshRootDirectories()
    {
        var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? new DirectoryInfo("/").GetDirectories()
            : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

        _rootDirectories.Edit(actions =>
        {
            actions.Clear();
            actions.AddOrUpdate(rootDirectories.Select(d => DirectoryToAbsolutePath(d, PointInTime.Present)));
        });
    }

    public override bool CanHandlePath(NativePath path)
    {
        var rootDrive = _rootDirectories
            .Items
            .FirstOrDefault(r =>
                path.Path.StartsWith(
                    GetNativePath(r.Path).Path,
                    _isCaseInsensitive
                        ? StringComparison.InvariantCultureIgnoreCase
                        : StringComparison.InvariantCulture
                )
            );

        return rootDrive is not null;
    }

    public override Task<IItem> GetItemByNativePathAsync(NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        var path = nativePath.Path;
        Exception? innerException;
        try
        {
            if ((path?.Length ?? 0) == 0)
            {
                return Task.FromResult((IItem) this);
            }
            else if (Directory.Exists(path))
            {
                return Task.FromResult((IItem) DirectoryToContainer(
                    new DirectoryInfo(path!.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar),
                    pointInTime,
                    !itemInitializationSettings.SkipChildInitialization)
                );
            }
            else if (File.Exists(path))
            {
                return Task.FromResult((IItem) FileToElement(new FileInfo(path), pointInTime));
            }

            var type = forceResolvePathType switch
            {
                AbsolutePathType.Container => "Directory",
                AbsolutePathType.Element => "File",
                _ => "Directory or file"
            };

            innerException = forceResolvePathType switch
            {
                AbsolutePathType.Container => new DirectoryNotFoundException($"{type} not found: '{path}'"),
                _ => new FileNotFoundException(type + " not found", path)
            };
        }
        catch (Exception e)
        {
            if (!forceResolve)
            {
                throw new Exception(
                    $"Could not resolve path '{nativePath.Path}' and {nameof(forceResolve)} is false.",
                    e
                );
            }

            innerException = e;
        }

        return forceResolvePathType switch
        {
            AbsolutePathType.Container => Task.FromResult(
                (IItem) CreateEmptyContainer(
                    nativePath,
                    pointInTime,
                    Observable.Return(new List<Exception>() {innerException})
                )
            ),
            AbsolutePathType.Element => Task.FromResult(CreateEmptyElement(nativePath)),
            _ => throw new Exception(
                $"Could not resolve path '{nativePath.Path}' and could not force create, because {nameof(forceResolvePathType)} is {nameof(AbsolutePathType.Unknown)}.",
                innerException)
        };
    }

    private Container CreateEmptyContainer(NativePath nativePath,
        PointInTime pointInTime,
        IObservable<IEnumerable<Exception>>? exceptions = null)
    {
        var nonNullExceptions = exceptions ?? Observable.Return(Enumerable.Empty<Exception>());
        var name = nativePath.Path.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "???";
        var fullName = GetFullName(nativePath);

        var parentFullName = fullName.GetParent();
        var parent = new AbsolutePath(
            _timelessContentProvider,
            pointInTime,
            parentFullName ?? new FullName(""),
            AbsolutePathType.Container);

        return new Container(
            name,
            name,
            fullName,
            nativePath,
            parent,
            false,
            true,
            DateTime.MinValue,
            SupportsDelete.False,
            false,
            "???",
            this,
            true,
            pointInTime,
            nonNullExceptions,
            new ExtensionCollection().AsReadOnly(),
            Observable.Return<IObservable<IChangeSet<AbsolutePath, string>>?>(null)
        );
    }

    private IItem CreateEmptyElement(NativePath nativePath)
    {
        throw new NotImplementedException();
    }

    public override Task<List<AbsolutePath>> GetItemsByContainerAsync(FullName fullName, PointInTime pointInTime)
        => Task.FromResult(GetItemsByContainer(fullName, pointInTime));

    private List<AbsolutePath> GetItemsByContainer(FullName fullName, PointInTime pointInTime)
        => GetItemsByContainer(new DirectoryInfo(GetNativePath(fullName).Path), pointInTime);

    private List<AbsolutePath> GetItemsByContainer(DirectoryInfo directoryInfo, PointInTime pointInTime)
        => directoryInfo
            .GetDirectories()
            .Select(d => DirectoryToAbsolutePath(d, pointInTime))
            .Concat(
                directoryInfo
                    .GetFiles()
                    .Select(f => FileToAbsolutePath(f, pointInTime))
            )
            .ToList();

    private AbsolutePath DirectoryToAbsolutePath(DirectoryInfo directoryInfo, PointInTime pointInTime)
    {
        var fullName = GetFullName(directoryInfo);
        return new AbsolutePath(_timelessContentProvider, pointInTime, fullName, AbsolutePathType.Container);
    }

    private AbsolutePath FileToAbsolutePath(FileInfo file, PointInTime pointInTime)
    {
        var fullName = GetFullName(file);
        return new AbsolutePath(_timelessContentProvider, pointInTime, fullName, AbsolutePathType.Element);
    }

    private Container DirectoryToContainer(DirectoryInfo directoryInfo, PointInTime pointInTime,
        bool initializeChildren = true)
    {
        var fullName = GetFullName(directoryInfo.FullName);
        var parentFullName = fullName.GetParent();
        var parent = new AbsolutePath(
            _timelessContentProvider,
            pointInTime,
            parentFullName ?? new FullName(""),
            AbsolutePathType.Container);
        var exceptions = new BehaviorSubject<IEnumerable<Exception>>(Enumerable.Empty<Exception>());

        return new Container(
            directoryInfo.Name,
            directoryInfo.Name,
            fullName,
            new NativePath(directoryInfo.FullName),
            parent,
            (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            directoryInfo.Exists,
            directoryInfo.CreationTime,
            SupportsDelete.True,
            true,
            GetDirectoryAttributes(directoryInfo),
            this,
            true,
            pointInTime,
            exceptions,
            new ExtensionCollection().AsReadOnly(),
            //Observable.FromAsync(async () => await Task.Run(InitChildrenHelper)
            Observable.Return(InitChildren())
        );

        Task<IObservable<IChangeSet<AbsolutePath, string>>?> InitChildrenHelper() => Task.FromResult(InitChildren());

        IObservable<IChangeSet<AbsolutePath, string>>? InitChildren()
        {
            if (!initializeChildren) return null;

            try
            {
                var items = GetItemsByContainer(directoryInfo, pointInTime);
                var result = new SourceCache<AbsolutePath, string>(i => i.Path.Path);

                if (items.Count == 0) return (IObservable<IChangeSet<AbsolutePath, string>>?) result.Connect().StartWithEmpty();

                result.AddOrUpdate(items);
                return (IObservable<IChangeSet<AbsolutePath, string>>?) result.Connect();
            }
            catch (Exception e)
            {
                exceptions.OnNext(new List<Exception> {e});
            }

            return null;
        }
    }

    private Element FileToElement(FileInfo fileInfo, PointInTime pointInTime)
    {
        var fullName = GetFullName(fileInfo);
        var parentFullName = fullName.GetParent() ??
                             throw new Exception($"Path does not have parent: '{fileInfo.FullName}'");
        var parent = new AbsolutePath(_timelessContentProvider, pointInTime, parentFullName,
            AbsolutePathType.Container);

        var extensions = new ExtensionCollection()
        {
            new FileExtension(fileInfo.Length)
        };

        return new Element(
            fileInfo.Name,
            fileInfo.Name,
            fullName,
            new NativePath(fileInfo.FullName),
            parent,
            (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            fileInfo.Exists,
            fileInfo.CreationTime,
            SupportsDelete.True,
            true,
            GetFileAttributes(fileInfo),
            this,
            pointInTime,
            Observable.Return(Enumerable.Empty<Exception>()),
            extensions.AsReadOnly()
        );
    }

    private FullName GetFullName(DirectoryInfo directoryInfo) => GetFullName(directoryInfo.FullName);

    private FullName GetFullName(FileInfo fileInfo) => GetFullName(fileInfo.FullName);
    private FullName GetFullName(NativePath nativePath) => GetFullName(nativePath.Path);

    private FullName GetFullName(string nativePath) =>
        new((Name + Constants.SeparatorChar +
             string.Join(Constants.SeparatorChar,
                 nativePath.TrimStart(Constants.SeparatorChar).Split(Path.DirectorySeparatorChar)))
            .TrimEnd(Constants.SeparatorChar));

    public override NativePath GetNativePath(FullName fullName)
    {
        var path = string.Join(Path.DirectorySeparatorChar, fullName.Path.Split(Constants.SeparatorChar).Skip(1));
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !path.StartsWith("/")) path = "/" + path;
        return new NativePath(path);
    }

    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return null;
        if (!File.Exists(element.NativePath!.Path))
            throw new FileNotFoundException("File does not exist", element.NativePath.Path);

        await using var reader = new FileStream(element.NativePath!.Path, FileMode.Open, FileAccess.Read,
            FileShare.Read,
            bufferSize: 1, // bufferSize == 1 used to avoid unnecessary buffer in FileStream
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var realFileSize = new FileInfo(element.NativePath!.Path).Length;

        var size = maxLength ?? realFileSize switch
        {
            > int.MaxValue => int.MaxValue,
            _ => (int) realFileSize
        };
        var buffer = new byte[size];
        await reader.ReadAsync(buffer.AsMemory(0, size), cancellationToken);

        return buffer;
    }
}