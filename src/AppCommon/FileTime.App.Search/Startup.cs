using FileTime.Core.ContentAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.Search;

public static class Startup
{
    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.TryAddSingleton<ISearchContentProvider, SearchContentProvider>();
        services.AddSingleton<IContentProvider>(sp => sp.GetRequiredService<ISearchContentProvider>());
        services.TryAddSingleton<ISearchManager, SearchManager>();

        return services;
    }
}