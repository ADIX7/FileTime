namespace FileTime.Core.ContentAccess;

public interface IContentAccessorFactory
{
    IItemCreator<TContentProvider> GetItemCreator<TContentProvider>() where TContentProvider : IContentProvider;
    IItemCreator GetItemCreator(IContentProvider provider);
}