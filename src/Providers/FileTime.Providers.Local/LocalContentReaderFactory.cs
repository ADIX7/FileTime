using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public class LocalContentReaderFactory : IContentReaderFactory<ILocalContentProvider>
{
    public Task<IContentReader> CreateContentReaderAsync(IElement element)
        => Task.FromResult((IContentReader)new LocalContentReader(File.OpenRead(element.NativePath!.Path)));
}