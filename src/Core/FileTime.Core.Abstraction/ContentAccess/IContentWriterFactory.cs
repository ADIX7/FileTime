using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IContentWriterFactory
{
    Task<IContentWriter> CreateContentWriterAsync(IElement element);
}

public interface IContentWriterFactory<in TContentProvider> : IContentWriterFactory where TContentProvider : IContentProvider
{
}