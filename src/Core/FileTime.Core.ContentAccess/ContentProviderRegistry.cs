using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.ContentAccess;

public class ContentProviderRegistry : IContentProviderRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Lazy<IList<IContentProvider>> _defaultContentProviders;
    private readonly List<IContentProvider> _additionalContentProviders = new();

    public ContentProviderRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultContentProviders = new Lazy<IList<IContentProvider>>(() => serviceProvider.GetServices<IContentProvider>().ToList());
    }

    public IEnumerable<IContentProvider> ContentProviders => _defaultContentProviders.Value.Concat(_additionalContentProviders);

    public void AddContentProvider(IContentProvider contentProvider) => _additionalContentProviders.Add(contentProvider);
    public void RemoveContentProvider(IContentProvider contentProvider) => _additionalContentProviders.Remove(contentProvider);
}