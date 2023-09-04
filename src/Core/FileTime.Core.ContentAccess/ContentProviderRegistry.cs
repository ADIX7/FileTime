using System.Collections.ObjectModel;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.ContentAccess;

public class ContentProviderRegistry : IContentProviderRegistry
{
    private readonly object _lock = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ObservableCollection<IContentProvider> _contentProviders = new();
    private readonly ObservableCollection<ISubContentProvider> _subContentProviders = new();
    private readonly ReadOnlyObservableCollection<IContentProvider> _contentProvidersReadOnly;
    private readonly ReadOnlyObservableCollection<ISubContentProvider> _subContentProvidersReadOnly;
    private bool _initialized;

    public ContentProviderRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _contentProvidersReadOnly = new ReadOnlyObservableCollection<IContentProvider>(_contentProviders);
        _subContentProvidersReadOnly = new ReadOnlyObservableCollection<ISubContentProvider>(_subContentProviders);
    }

    public ReadOnlyObservableCollection<IContentProvider> ContentProviders
    {
        get
        {
            InitializeContentProviderListIfNeeded();
            return _contentProvidersReadOnly;
        }
    }

    public ReadOnlyObservableCollection<ISubContentProvider> SubContentProviders
    {
        get
        {
            InitializeContentProviderListIfNeeded();
            return _subContentProvidersReadOnly;
        }
    }

    private void InitializeContentProviderListIfNeeded()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;

            foreach (var contentProvider in _serviceProvider.GetServices<IContentProvider>())
            {
                _contentProviders.Add(contentProvider);
            }

            foreach (var subContentProvider in _serviceProvider.GetServices<ISubContentProvider>())
            {
                _subContentProviders.Add(subContentProvider);
            }

            _initialized = true;
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

    public async Task<ISubContentProvider?> GetSubContentProviderForElement(IElement parentElement)
    {
        var subContentProviders = _serviceProvider
            .GetServices<ISubContentProvider>()
            .ToList();

        foreach (var subContentProvider in subContentProviders)
        {
            if(!await subContentProvider.CanHandleAsync(parentElement)) continue;
            
            return subContentProvider;
        }

        return null;
    }
}