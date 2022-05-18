using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using DynamicData;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Services;

namespace FileTime.Providers.Local;

public sealed partial class LocalContentProvider : ContentProviderBase, ILocalContentProvider
{
    private readonly SourceList<IAbsolutePath> _rootDirectories = new();
    private readonly bool _isCaseInsensitive;

    public LocalContentProvider() : base("local")
    {
        _isCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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
            actions.AddRange(rootDirectories.Select(DirectoryToAbsolutePath));
        });
    }

    public override Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
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
                return Task.FromResult((IItem)this);
            }
            else if (Directory.Exists(path))
            {
                return Task.FromResult((IItem)DirectoryToContainer(
                    new DirectoryInfo(path!.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar),
                    !itemInitializationSettings.SkipChildInitialization)
                );
            }
            else if (File.Exists(path))
            {
                return Task.FromResult((IItem)FileToElement(new FileInfo(path)));
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
                (IItem)CreateEmptyContainer(nativePath, Observable.Return(new List<Exception>() { innerException }))
            ),
            AbsolutePathType.Element => Task.FromResult(CreateEmptyElement(nativePath)),
            _ => throw new Exception(
                $"Could not resolve path '{nativePath.Path}' and could not force create, because {nameof(forceResolvePathType)} is {nameof(AbsolutePathType.Unknown)}.",
                innerException)
        };
    }

    private Container CreateEmptyContainer(NativePath nativePath,
        IObservable<IEnumerable<Exception>>? exceptions = null)
    {
        var nonNullExceptions = exceptions ?? Observable.Return(Enumerable.Empty<Exception>());
        var name = nativePath.Path.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "???";
        var fullName = GetFullName(nativePath);

        var parentFullName = fullName.GetParent();
        var parent = new AbsolutePath(
            this,
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
            nonNullExceptions,
            Observable.Return<IObservable<IChangeSet<IAbsolutePath>>?>(null)
        );
    }

    private IItem CreateEmptyElement(NativePath nativePath)
    {
        throw new NotImplementedException();
    }

    public override Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName)
        => Task.FromResult(GetItemsByContainer(fullName));

    private List<IAbsolutePath> GetItemsByContainer(FullName fullName)
        => GetItemsByContainer(new DirectoryInfo(GetNativePath(fullName).Path));

    private List<IAbsolutePath> GetItemsByContainer(DirectoryInfo directoryInfo)
        => directoryInfo
            .GetDirectories()
            .Select(DirectoryToAbsolutePath)
            .Concat(
                directoryInfo
                    .GetFiles()
                    .Select(FileToAbsolutePath)
            )
            .ToList();

    private IAbsolutePath DirectoryToAbsolutePath(DirectoryInfo directoryInfo)
    {
        var fullName = GetFullName(directoryInfo);
        return new AbsolutePath(this, fullName, AbsolutePathType.Container);
    }

    private IAbsolutePath FileToAbsolutePath(FileInfo file)
    {
        var fullName = GetFullName(file);
        return new AbsolutePath(this, fullName, AbsolutePathType.Element);
    }

    private Container DirectoryToContainer(DirectoryInfo directoryInfo, bool initializeChildren = true)
    {
        var fullName = GetFullName(directoryInfo.FullName);
        var parentFullName = fullName.GetParent();
        var parent = new AbsolutePath(
            this,
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
            exceptions,
            Observable.FromAsync(async () => await Task.Run(InitChildren))
        );

        Task<IObservable<IChangeSet<IAbsolutePath>>?> InitChildren()
        {
            SourceList<IAbsolutePath>? result = null;
            try
            {
                var items = initializeChildren ? (List<IAbsolutePath>?)GetItemsByContainer(directoryInfo) : null;
                if (items != null)
                {
                    result = new SourceList<IAbsolutePath>();
                    result.AddRange(items);
                }
            }
            catch (Exception e)
            {
                exceptions.OnNext(new List<Exception>() { e });
            }

            return Task.FromResult(result?.Connect());
        }
    }

    private Element FileToElement(FileInfo fileInfo)
    {
        var fullName = GetFullName(fileInfo);
        var parentFullName = fullName.GetParent() ??
                             throw new Exception($"Path does not have parent: '{fileInfo.FullName}'");
        var parent = new AbsolutePath(this, parentFullName, AbsolutePathType.Container);

        return new FileElement(
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
            Observable.Return(Enumerable.Empty<Exception>()),
            fileInfo.Length
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
}