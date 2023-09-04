using System.Collections.ObjectModel;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using SharpCompress.Archives;
using IContainer = FileTime.Core.Models.IContainer;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedSubContentProvider : ICompressedSubContentProvider
{
    private static readonly string[] SupportedExtensions = {".zip", ".gz", ".7z"};

    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ICompressedContentProviderFactory _compressedContentProviderFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;

    public CompressedSubContentProvider(
        IContentAccessorFactory contentAccessorFactory,
        ICompressedContentProviderFactory compressedContentProviderFactory,
        ITimelessContentProvider timelessContentProvider
    )
    {
        _contentAccessorFactory = contentAccessorFactory;
        _compressedContentProviderFactory = compressedContentProviderFactory;
        _timelessContentProvider = timelessContentProvider;
    }

    public Task<bool> CanHandleAsync(IElement parentElement)
        => Task.FromResult(
            parentElement.NativePath?.Path is { } path
            && SupportedExtensions.Any(e => path.EndsWith(e))
        );

    public async Task<IItem?> GetItemByFullNameAsync(
        IElement parentElement,
        FullName itemPath,
        PointInTime pointInTime,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        var parentContentReader = await _contentAccessorFactory.GetContentReaderFactory(parentElement.Provider).CreateContentReaderAsync(parentElement);
        var parentContentReaderStream = parentContentReader.AsStream();
        var archive = ArchiveFactory.Open(parentContentReaderStream);
        var disposables = new IDisposable[] {parentContentReader, parentContentReaderStream, archive};

        if (itemPath.Path.Length == 0 || itemPath.Path == Constants.SubContentProviderRootContainer)
        {
            var rootFullNameBase = parentElement.FullName!.Path + Constants.SeparatorChar + Constants.SubContentProviderRootContainer;
            var rootNativePathBase = parentElement.NativePath!.Path + Constants.SeparatorChar + Constants.SubContentProviderRootContainer;

            var rootFullName = new FullName(rootFullNameBase);
            var rootNativePath = new NativePath(rootNativePathBase);

            var container = CreateContainer(
                archive,
                new FullName(":/"),
                rootFullName,
                rootNativePath,
                parentElement,
                parentElement.Parent!,
                itemInitializationSettings,
                disposables
            );
            return container;
        }

        return ResolveNonRootChild(
            archive,
            parentElement,
            itemPath,
            itemInitializationSettings,
            disposables
        );
    }

    private IItem ResolveNonRootChild(
        IArchive archive,
        IElement parentElement,
        FullName itemPath,
        ItemInitializationSettings itemInitializationSettings,
        ICollection<IDisposable> disposables)
    {
        var childFullNameBase = parentElement.FullName!.Path + Constants.SeparatorChar + itemPath.Path;
        var childNativePathBase = parentElement.NativePath!.Path + Constants.SeparatorChar + itemPath.Path;

        var childFullName = new FullName(childFullNameBase);
        var childNativePath = new NativePath(childNativePathBase);

        var isDirectory = false;
        var path = itemPath.Path
            .Substring(1 + Constants.SubContentProviderRootContainer.Length)
            .Replace(Constants.SeparatorChar, '/');

        var pathWithSlash = path + '/';
        var size = 0L;
        foreach (var archiveEntry in archive.Entries)
        {
            if (archiveEntry.Key == path)
            {
                if (archiveEntry.IsDirectory)
                {
                    isDirectory = true;
                    break;
                }

                size = archiveEntry.Size;
                break;
            }

            if (archiveEntry.Key.StartsWith(pathWithSlash))
            {
                isDirectory = true;
                break;
            }
        }

        var parent = new AbsolutePath(
            _timelessContentProvider,
            parentElement.PointInTime,
            childFullName.GetParent()!,
            AbsolutePathType.Container
        );
        
        if (isDirectory)
        {
            return CreateContainer(
                archive,
                itemPath,
                childFullName,
                childNativePath,
                parentElement,
                parent,
                itemInitializationSettings,
                disposables
            );
        }
        else
        {
            var element = CreateElement(
                itemPath,
                childFullName,
                childNativePath,
                parentElement,
                parent,
                size);

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            return element;
        }
    }

    private IContainer CreateContainer(
        IArchive archive,
        FullName itemPath,
        FullName fullName,
        NativePath nativePath,
        IElement parentElement,
        AbsolutePath parent,
        ItemInitializationSettings initializationSettings,
        ICollection<IDisposable> disposables)
    {
        var name = itemPath.Path.Split(Constants.SeparatorChar).Last();

        var children = new ObservableCollection<AbsolutePath>();
        var exceptions = new ObservableCollection<Exception>();

        var container = new Container(
            name,
            name,
            fullName,
            nativePath,
            parent,
            true,
            true,
            parentElement.CreatedAt,
            parentElement.ModifiedAt,
            SupportsDelete.False,
            false,
            "",
            _compressedContentProviderFactory.Create(parentElement.Provider),
            false,
            parentElement.PointInTime,
            exceptions,
            new ReadOnlyExtensionCollection(new ExtensionCollection()),
            children
        );

        if (!initializationSettings.SkipChildInitialization)
        {
            LoadChildren(archive, container, itemPath, parentElement.PointInTime, children, exceptions);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    container.StartLoading();
                    //LoadChildren(archive, container, itemPath, parentElement.PointInTime, children, exceptions);
                }
                finally
                {
                    container.StopLoading();
                    foreach (var disposable in disposables)
                    {
                        disposable.Dispose();
                    }
                }
            });
        }

        return container;
    }

    private void LoadChildren(
        IArchive archive,
        Container container,
        FullName itemPath,
        PointInTime pointInTime,
        ObservableCollection<AbsolutePath> children,
        ObservableCollection<Exception> exceptions)
    {
        var containerPath = itemPath.Path
            .Substring(1 + Constants.SubContentProviderRootContainer.Length);
        var containerLevel = containerPath.Length != 0
            ? containerPath.Split(Constants.SeparatorChar).Length
            : 0;

        var addedContainers = new List<string>();
        foreach (var archiveEntry in archive.Entries)
        {
            if (!archiveEntry.Key.StartsWith(containerPath)) continue;

            var childPathParts = archiveEntry.Key.TrimEnd('/').Split('/');
            if (childPathParts.Length < containerLevel + 1) continue;
            var itemName = childPathParts[containerLevel];

            if ((archiveEntry.IsDirectory && childPathParts.Length == containerLevel + 1)
                || (!archiveEntry.IsDirectory && childPathParts.Length > containerLevel + 1))
            {
                //Container
                if (addedContainers.Contains(itemName)) continue;
                addedContainers.Add(itemName);

                children.Add(new AbsolutePath(
                    _timelessContentProvider,
                    pointInTime,
                    container.FullName.GetChild(itemName),
                    AbsolutePathType.Container)
                );
            }
            else if (!archiveEntry.IsDirectory && childPathParts.Length == containerLevel + 1)
            {
                //Element
                children.Add(new AbsolutePath(
                    _timelessContentProvider,
                    pointInTime,
                    container.FullName.GetChild(itemName),
                    AbsolutePathType.Element)
                );
            }
        }
    }

    private IItem CreateElement(FullName itemPath,
        FullName fullName,
        NativePath nativePath,
        IElement parentElement,
        AbsolutePath parent,
        long size)
    {
        var name = itemPath.Path.Split(Constants.SeparatorChar).Last();

        var exceptions = new ObservableCollection<Exception>();

        var element = new Element(
            name,
            name,
            fullName,
            nativePath,
            parent,
            true,
            true,
            parentElement.CreatedAt,
            parentElement.ModifiedAt,
            SupportsDelete.False,
            false,
            "",
            size,
            _compressedContentProviderFactory.Create(parentElement.Provider),
            parentElement.PointInTime,
            exceptions,
            new ReadOnlyExtensionCollection(new ExtensionCollection())
        );

        return element;
    }
}