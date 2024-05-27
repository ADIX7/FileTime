using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Local;

public sealed partial class LocalContentProvider : ContentProviderBase, ILocalContentProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly bool _isCaseInsensitive;
    private readonly Lazy<IRootDriveInfoService> _rootDriveInfo;

    public LocalContentProvider(
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider,
        IContentProviderRegistry contentProviderRegistry)
        : base(LocalContentProviderConstants.ContentProviderId, timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentProviderRegistry = contentProviderRegistry;
        _isCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        _rootDriveInfo = new Lazy<IRootDriveInfoService>(serviceProvider.GetRequiredService<IRootDriveInfoService>);

        SupportsContentStreams = true;

        RefreshRootDirectories();
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

        Items.Clear();
        Items.AddRange(rootDirectories.Select(d => DirectoryToAbsolutePath(d, PointInTime.Present)));
    }

    public override async Task<bool> CanHandlePathAsync(NativePath path)
    {
        var rootDrive = await Items
            .ToAsyncEnumerable()
            .FirstOrDefaultAwaitAsync(async r =>
                path.Path.StartsWith(
                    (await GetNativePathAsync(r.Path)).Path,
                    _isCaseInsensitive
                        ? StringComparison.InvariantCultureIgnoreCase
                        : StringComparison.InvariantCulture
                )
            );

        return rootDrive is not null;
    }

    public override ValueTask<VolumeSizeInfo?> GetVolumeSizeInfoAsync(FullName path)
    {
        var nativePath = GetNativePath(path);
        
        var possibleRootDrives =  _rootDriveInfo.Value.AllDrives.Where(d => nativePath.Path.StartsWith(d.RootDirectory.FullName)).ToArray();
        var rootDrive = possibleRootDrives.Length == 0
            ? null
            : possibleRootDrives.MaxBy(d => d.RootDirectory.FullName.Length);
        
        return rootDrive is null 
            ? ValueTask.FromResult<VolumeSizeInfo?>(null) 
            : ValueTask.FromResult<VolumeSizeInfo?>(new VolumeSizeInfo(rootDrive.TotalSize, rootDrive.AvailableFreeSpace));

        /*var rootDriveInfos = _rootDriveInfo.Value;
        var possibleRootDriveInfo = rootDriveInfos.RootDriveInfos.Where(d => path.Path.StartsWith(d.Path.Path)).ToArray();
        var rootDriveInfo = possibleRootDriveInfo.Length == 0
            ? null
            : possibleRootDriveInfo.MaxBy(d => d.FullName.Length);

        if (rootDriveInfo is null) return ValueTask.FromResult<VolumeSizeInfo?>(null);

        return ValueTask.FromResult<VolumeSizeInfo?>(new VolumeSizeInfo(rootDriveInfo.Size, rootDriveInfo.Free));*/
    }

    public override async Task<IItem> GetItemByNativePathAsync(NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        var path = nativePath.Path;
        Exception? innerException;
        try
        {
            if (path.Length == 0)
            {
                return this;
            }

            if (Directory.Exists(path))
            {
                return DirectoryToContainer(
                    new DirectoryInfo(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar),
                    pointInTime,
                    itemInitializationSettings);
            }
            else if (File.Exists(path))
            {
                return FileToElement(new FileInfo(path), pointInTime);
            }

            var pathParts = path.Split(Path.DirectorySeparatorChar).SelectMany(p => p.Split(Constants.SeparatorChar))
                .ToArray();

            for (var i = pathParts.Length - 1; i > 0; i--)
            {
                var possibleFile = string.Join(Path.DirectorySeparatorChar, pathParts.Take(i));
                if (!File.Exists(possibleFile)) continue;

                var element = FileToElement(new FileInfo(possibleFile), pointInTime);
                var subContentProvider = await _contentProviderRegistry.GetSubContentProviderForElement(element);
                if (subContentProvider is null) break;

                var subPath = string.Join(Constants.SeparatorChar, pathParts.Skip(i));

                var resolvedItem = await subContentProvider.GetItemByFullNameAsync(element, new FullName(subPath),
                    pointInTime, forceResolvePathType, itemInitializationSettings);

                if (resolvedItem is not null)
                {
                    return resolvedItem;
                }
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
            AbsolutePathType.Container => CreateEmptyContainer(
                nativePath,
                pointInTime,
                new List<Exception> { innerException }
            ),
            AbsolutePathType.Element => CreateEmptyElement(nativePath),
            _ => throw new Exception(
                $"Could not resolve path '{nativePath.Path}' and could not force create, because {nameof(forceResolvePathType)} is {nameof(AbsolutePathType.Unknown)}.",
                innerException)
        };
    }

    public override ValueTask<NativePath?> GetSupportedPathPart(NativePath nativePath)
    {
        var path = nativePath.Path;
        var pathParts = path.Split(Path.DirectorySeparatorChar).SelectMany(p => p.Split(Constants.SeparatorChar))
            .ToArray();

        for (var i = pathParts.Length - 1; i > 0; i--)
        {
            var possiblePath = string.Join(Path.DirectorySeparatorChar, pathParts.Take(i));
            if (!File.Exists(possiblePath) && !Directory.Exists(possiblePath)) continue;

            return ValueTask.FromResult<NativePath?>(new NativePath(possiblePath));
        }

        return ValueTask.FromResult<NativePath?>(null);
    }

    private Container CreateEmptyContainer(NativePath nativePath,
        PointInTime pointInTime,
        IEnumerable<Exception>? initialExceptions = null)
    {
        var exceptions = new ObservableCollection<Exception>();
        if (initialExceptions is not null)
        {
            exceptions.AddRange(initialExceptions);
        }

        var name = nativePath.Path.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "???";
        var fullName = GetFullName(nativePath);

        var parentFullName = fullName.GetParent();
        var parent =
            parentFullName is null
                ? null
                : new AbsolutePath(
                    _timelessContentProvider,
                    pointInTime,
                    parentFullName,
                    AbsolutePathType.Container);

        var container = new Container(
            name,
            name,
            fullName,
            nativePath,
            parent,
            false,
            true,
            DateTime.MinValue,
            DateTime.MinValue,
            SupportsDelete.False,
            false,
            "???",
            this,
            true,
            pointInTime,
            exceptions,
            new ExtensionCollection().AsReadOnly(),
            new ObservableCollection<AbsolutePath>()
        );
        container.StopLoading();
        return container;
    }

    private IItem CreateEmptyElement(NativePath nativePath)
    {
        throw new NotImplementedException();
    }

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
        ItemInitializationSettings initializationSettings = default)
    {
        var fullName = GetFullName(directoryInfo.FullName);
        var parentFullName = fullName.GetParent();
        var parent =
            initializationSettings.Parent
            ?? (parentFullName is null
                ? null
                : new AbsolutePath(
                    _timelessContentProvider,
                    pointInTime,
                    parentFullName,
                    AbsolutePathType.Container));
        var exceptions = new ObservableCollection<Exception>();

        var children = new ObservableCollection<AbsolutePath>();

        var container = new Container(
            directoryInfo.Name,
            directoryInfo.Name,
            fullName,
            new NativePath(directoryInfo.FullName),
            parent,
            (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            directoryInfo.Exists,
            directoryInfo.CreationTime,
            directoryInfo.LastWriteTime,
            SupportsDelete.True,
            true,
            GetDirectoryAttributes(directoryInfo),
            this,
            true,
            pointInTime,
            exceptions,
            new ExtensionCollection().AsReadOnly(),
            children
        );

        if (!initializationSettings.SkipChildInitialization)
        {
            Task.Run(async () => await LoadChildren(container, directoryInfo, children, pointInTime, exceptions));
        }
        else
        {
            container.StopLoading();
        }

        return container;
    }

    private async Task LoadChildren(Container container,
        DirectoryInfo directoryInfo,
        ObservableCollection<AbsolutePath> children,
        PointInTime pointInTime,
        ObservableCollection<Exception> exceptions)
    {
        var lockObj = new object();
        var loadingIndicatorCancellation = new CancellationTokenSource();

#pragma warning disable CS4014
        Task.Run(async () => await DelayedLoadingIndicator());
#pragma warning restore CS4014
        await LoadChildrenInternal();

        lock (lockObj)
        {
            loadingIndicatorCancellation.Cancel();
            container.StopLoading();
        }

        Task LoadChildrenInternal()
        {
            try
            {
                foreach (var directory in directoryInfo.EnumerateDirectories())
                {
                    try
                    {
                        if (container.LoadingCancellationToken.IsCancellationRequested) break;
                        var absolutePath = DirectoryToAbsolutePath(directory, pointInTime);
                        children.Add(absolutePath);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }

                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    try
                    {
                        if (container.LoadingCancellationToken.IsCancellationRequested) break;
                        var absolutePath = FileToAbsolutePath(file, pointInTime);
                        children.Add(absolutePath);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

            return Task.CompletedTask;
        }

        async Task DelayedLoadingIndicator()
        {
            var token = loadingIndicatorCancellation.Token;
            try
            {
                await Task.Delay(2000, token);
            }
            catch
            {
            }

            lock (lockObj)
            {
                if (token.IsCancellationRequested) return;
                container.StartLoading();
            }
        }
    }

    private Element FileToElement(FileInfo fileInfo, PointInTime pointInTime)
    {
        var fullName = GetFullName(fileInfo);
        var parentFullName = fullName.GetParent() ??
                             throw new Exception($"Path does not have parent: '{fileInfo.FullName}'");
        var parent = new AbsolutePath(_timelessContentProvider, pointInTime, parentFullName,
            AbsolutePathType.Container);

        return new Element(
            fileInfo.Name,
            fileInfo.Name,
            fullName,
            new NativePath(fileInfo.FullName),
            parent,
            (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            fileInfo.Exists,
            fileInfo.CreationTime,
            fileInfo.LastWriteTime,
            SupportsDelete.True,
            true,
            GetFileAttributes(fileInfo),
            fileInfo.Length,
            this,
            pointInTime,
            new ObservableCollection<Exception>(),
            new ExtensionCollection().AsReadOnly()
        );
    }

    private FullName GetFullName(DirectoryInfo directoryInfo) => GetFullName(directoryInfo.FullName);

    private FullName GetFullName(FileInfo fileInfo) => GetFullName(fileInfo.FullName);
    public override FullName GetFullName(NativePath nativePath) => GetFullName(nativePath.Path);

    private FullName GetFullName(string nativePath) =>
        FullName.CreateSafe((Name + Constants.SeparatorChar +
                             string.Join(Constants.SeparatorChar,
                                 nativePath.TrimStart(Constants.SeparatorChar).Split(Path.DirectorySeparatorChar)))
            .TrimEnd(Constants.SeparatorChar))!;

    public override ValueTask<NativePath> GetNativePathAsync(FullName fullName)
        => ValueTask.FromResult(GetNativePath(fullName));

    public NativePath GetNativePath(FullName fullName)
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

        var size = new FileInfo(element.NativePath!.Path).Length;

        if (maxLength.HasValue && maxLength.Value < size)
        {
            size = maxLength.Value;
        }

        var finalSize = size switch
        {
            > int.MaxValue => int.MaxValue,
            _ => (int)size
        };
        var buffer = new byte[finalSize];
        var realSize = await reader.ReadAsync(buffer.AsMemory(0, finalSize), cancellationToken);

        if (realSize == buffer.Length) return buffer;

        var finalData = new byte[realSize];
        Array.Copy(buffer, finalData, realSize);

        return buffer;
    }
}