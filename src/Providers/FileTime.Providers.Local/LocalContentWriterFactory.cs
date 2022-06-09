using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public class LocalContentWriterFactory : IContentWriterFactory<ILocalContentProvider>
{
    public Task<IContentWriter> CreateContentWriterAsync(IElement element)
        => Task.FromResult((IContentWriter)new LocalContentWriter(File.OpenWrite(element.NativePath!.Path)));
}