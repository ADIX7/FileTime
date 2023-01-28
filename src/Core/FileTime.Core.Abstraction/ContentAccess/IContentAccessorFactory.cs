namespace FileTime.Core.ContentAccess;

public interface IContentAccessorFactory
{
    IItemCreator<TContentProvider> GetItemCreator<TContentProvider>() where TContentProvider : IContentProvider;
    IItemCreator GetItemCreator(IContentProvider provider);
    IContentReaderFactory<TContentProvider> GetContentReaderFactory<TContentProvider>() where TContentProvider : IContentProvider;
    IContentReaderFactory GetContentReaderFactory(IContentProvider provider);
    IContentWriterFactory<TContentProvider> GetContentWriterFactory<TContentProvider>() where TContentProvider : IContentProvider;
    IContentWriterFactory GetContentWriterFactory(IContentProvider provider);
    IItemDeleter GetItemDeleter(IContentProvider provider);
    IItemDeleter<TContentProvider> GetItemDeleter<TContentProvider>() where TContentProvider : IContentProvider;
}