using System.Collections.ObjectModel;
using DiscUtils;
using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskSubContentProvider : IVirtualDiskSubContentProvider
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IVirtualDiskContentProviderFactory _virtualDiskContentProviderFactory;

    public VirtualDiskSubContentProvider(
        IContentAccessorFactory contentAccessorFactory,
        ITimelessContentProvider timelessContentProvider,
        IVirtualDiskContentProviderFactory virtualDiskContentProviderFactory
    )
    {
        _contentAccessorFactory = contentAccessorFactory;
        _timelessContentProvider = timelessContentProvider;
        _virtualDiskContentProviderFactory = virtualDiskContentProviderFactory;
    }

    public Task<bool> CanHandleAsync(IElement parentElement)
        => Task.FromResult(parentElement.NativePath?.Path.EndsWith(".iso", StringComparison.OrdinalIgnoreCase) ?? false);

    public async Task<IItem?> GetItemByFullNameAsync(
        IElement parentElement,
        FullName itemPath,
        PointInTime pointInTime,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        var contentReaderFactory = _contentAccessorFactory.GetContentReaderFactory(parentElement.Provider);
        var reader = await contentReaderFactory.CreateContentReaderAsync(parentElement);

        await using var readerStream = reader.AsStream();
        var discReader = new UdfReader(readerStream);

        if (itemPath.Path.Length == 0 || itemPath.Path == Constants.SubContentProviderRootContainer)
        {
            var rootFullNameBase = parentElement.FullName!.Path + Constants.SeparatorChar + Constants.SubContentProviderRootContainer;
            var rootNativePathBase = parentElement.NativePath!.Path + Constants.SeparatorChar + Constants.SubContentProviderRootContainer;

            var rootFullName = new FullName(rootFullNameBase);
            var rootNativePath = new NativePath(rootNativePathBase);

            var container = CreateContainer(
                discReader,
                discReader.Root,
                rootFullName,
                rootNativePath,
                parentElement.Provider,
                parentElement.Parent!,
                parentElement.PointInTime,
                itemInitializationSettings);
            return container;
        }

        return ResolveNonRootChild(discReader, parentElement, itemPath, pointInTime, itemInitializationSettings);
    }

    private IItem? ResolveNonRootChild(
        UdfReader discReader,
        IElement parentElement,
        FullName itemPath,
        PointInTime pointInTime,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        var pathParts = itemPath.Path.Split(Constants.SeparatorChar);

        var childFullNameBase = parentElement.FullName!.Path + Constants.SeparatorChar + itemPath.Path;
        var childNativePathBase = parentElement.NativePath!.Path + Constants.SeparatorChar + itemPath.Path;

        var childFullName = new FullName(childFullNameBase);
        var childNativePath = new NativePath(childNativePathBase);

        var parent = new AbsolutePath(_timelessContentProvider, pointInTime, childFullName.GetParent()!, AbsolutePathType.Container);

        var container = discReader.Root;
        for (var i = 1; i < pathParts.Length - 1; i++)
        {
            if (container is null) break;
            container = container.GetDirectories().FirstOrDefault(d => d.Name == pathParts[i]);
        }

        if (container is null) return null;

        if (container.GetDirectories().FirstOrDefault(d => d.Name == pathParts[^1]) is { } childContainer)
        {
            return CreateContainer(
                discReader,
                childContainer,
                childFullName,
                childNativePath,
                parentElement.Provider,
                parent,
                pointInTime,
                itemInitializationSettings
            );
        }

        if (container.GetFiles().FirstOrDefault(d => d.Name == pathParts[^1]) is not { } childElement)
        {
            return null;
        }

        var element = CreateElement(
            childElement,
            childFullName,
            childNativePath,
            parentElement.Provider,
            parent,
            pointInTime
        );

        discReader.Dispose();
        return element;
    }

    private IContainer CreateContainer(
        UdfReader discReader,
        DiscDirectoryInfo sourceContainer,
        FullName fullname,
        NativePath nativePath,
        IContentProvider parentContentProvider,
        AbsolutePath parent,
        PointInTime pointInTime,
        ItemInitializationSettings initializationSettings)
    {
        var children = new ObservableCollection<AbsolutePath>();
        var exceptions = new ObservableCollection<Exception>();
        var container = new Container(
            sourceContainer.Name,
            sourceContainer.Name,
            fullname,
            nativePath,
            parent,
            true,
            true,
            sourceContainer.CreationTime,
            sourceContainer.LastWriteTime,
            SupportsDelete.False,
            false,
            FormatAttributes(sourceContainer.Attributes),
            _virtualDiskContentProviderFactory.Create(parentContentProvider),
            false,
            pointInTime,
            exceptions,
            new ReadOnlyExtensionCollection(new ExtensionCollection()),
            children
        );

        if (!initializationSettings.SkipChildInitialization)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                LoadChildren(container, sourceContainer, children, pointInTime, exceptions);
                discReader.Dispose();
            });
        }

        return container;
    }

    private void LoadChildren(
        Container container,
        DiscDirectoryInfo sourceContainer,
        ObservableCollection<AbsolutePath> children,
        PointInTime pointInTime,
        ObservableCollection<Exception> exceptions
    )
    {
        foreach (var discDirectoryInfo in sourceContainer.GetDirectories())
        {
            children.Add(new AbsolutePath(
                _timelessContentProvider,
                pointInTime,
                container.FullName.GetChild(discDirectoryInfo.Name),
                AbsolutePathType.Container)
            );
        }

        foreach (var fileInfo in sourceContainer.GetFiles())
        {
            children.Add(new AbsolutePath(
                _timelessContentProvider,
                pointInTime,
                container.FullName.GetChild(fileInfo.Name),
                AbsolutePathType.Element)
            );
        }
    }

    private IElement CreateElement(DiscFileInfo childElement,
        FullName fullname,
        NativePath nativePath,
        IContentProvider parentContentProvider,
        AbsolutePath parent,
        PointInTime pointInTime)
    {
        var element = new Element(
            childElement.Name,
            childElement.Name,
            fullname,
            nativePath,
            parent,
            true,
            true,
            childElement.CreationTime,
            childElement.LastWriteTime,
            SupportsDelete.False,
            false,
            FormatAttributes(childElement.Attributes),
            childElement.Length,
            _virtualDiskContentProviderFactory.Create(parentContentProvider),
            pointInTime,
            new ObservableCollection<Exception>(),
            new ReadOnlyExtensionCollection(new ExtensionCollection())
        );

        return element;
    }

    private string FormatAttributes(FileAttributes attributes)
    {
        return "";
    }
}