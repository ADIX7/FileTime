using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.ContentAccess;

public class ContentProviderRegistry : IContentProviderRegistry
{
    private readonly object _lock = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ObservableCollection<IContentProvider> _contentProviders = new();
    private readonly ReadOnlyObservableCollection<IContentProvider> _contentProvidersReadOnly;
    private bool _initialized;

    public ContentProviderRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _contentProvidersReadOnly = new ReadOnlyObservableCollection<IContentProvider>(_contentProviders);
    }

    public ReadOnlyObservableCollection<IContentProvider> ContentProviders
    {
        get
        {
            InitializeContentProviderListIfNeeded();
            return _contentProvidersReadOnly;
        }
    }

    private void InitializeContentProviderListIfNeeded()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (!_initialized)
            {
                foreach (var contentProvider in _serviceProvider.GetServices<IContentProvider>())
                {
                    _contentProviders.Add(contentProvider);
                }

                _initialized = true;
            }
        }
    }

    public void AddContentProvider(IContentProvider contentProvider)
    {
        InitializeContentProviderListIfNeeded();

        lock (_lock)
        {
            _contentProviders.Add(contentProvider);
        }
    }

    public void RemoveContentProvider(IContentProvider contentProvider)
    {
        InitializeContentProviderListIfNeeded();

        lock (_lock)
        {
            _contentProviders.Remove(contentProvider);
        }
    }
}