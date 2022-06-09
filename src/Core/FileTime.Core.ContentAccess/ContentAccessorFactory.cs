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

    public IContentReaderFactory<TContentProvider> GetContentReaderFactory<TContentProvider>() where TContentProvider : IContentProvider
    {
        var genericType = typeof(IContentReaderFactory<>).MakeGenericType(typeof(TContentProvider));

        return (IContentReaderFactory<TContentProvider>)_serviceProvider.GetRequiredService(genericType);
    }

    public IContentReaderFactory GetContentReaderFactory(IContentProvider provider)
    {
        var genericType = typeof(IContentReaderFactory<>).MakeGenericType(provider.GetType());

        return (IContentReaderFactory)_serviceProvider.GetRequiredService(genericType);
    }

    public IContentWriterFactory<TContentProvider> GetContentWriterFactory<TContentProvider>() where TContentProvider : IContentProvider
    {
        var genericType = typeof(IContentWriterFactory<>).MakeGenericType(typeof(TContentProvider));

        return (IContentWriterFactory<TContentProvider>)_serviceProvider.GetRequiredService(genericType);
    }

    public IContentWriterFactory GetContentWriterFactory(IContentProvider provider)
    {
        var genericType = typeof(IContentWriterFactory<>).MakeGenericType(provider.GetType());

        return (IContentWriterFactory)_serviceProvider.GetRequiredService(genericType);
    }
}