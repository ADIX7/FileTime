using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IContentReaderFactory
{
    Task<IContentReader> CreateContentReaderAsync(IElement element);
}

public interface IContentReaderFactory<in TContentProvider> : IContentReaderFactory where TContentProvider : IContentProvider
{
}