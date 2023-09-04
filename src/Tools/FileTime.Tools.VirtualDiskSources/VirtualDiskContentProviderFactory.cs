﻿using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskContentProviderFactory : IVirtualDiskContentProviderFactory
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public VirtualDiskContentProviderFactory(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
    }
    
    public IVirtualDiskContentProvider Create(IContentProvider parentContentProvider)
        => new VirtualDiskContentProvider(parentContentProvider, _timelessContentProvider, _contentAccessorFactory);
}