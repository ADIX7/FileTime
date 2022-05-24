using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.ContentAccess;

public class ContentAccessorFactory : IContentAccessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ContentAccessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IItemCreator<TContentProvider> GetItemCreator<TContentProvider>() where TContentProvider : IContentProvider
    {
        var genericType = typeof(IItemCreator<>).MakeGenericType(typeof(TContentProvider));

        return (IItemCreator<TContentProvider>)_serviceProvider.GetRequiredService(genericType);
    }
    
    public IItemCreator GetItemCreator(IContentProvider provider)
    {
        var genericType = typeof(IItemCreator<>).MakeGenericType(provider.GetType());

        return (IItemCreator)_serviceProvider.GetRequiredService(genericType);
    }
}