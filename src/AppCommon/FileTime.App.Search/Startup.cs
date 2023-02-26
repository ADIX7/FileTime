using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Search;

public static class Startup
{
    public static IServiceCollection AddSearch(this IServiceCollection services)
    {
        services.AddSingleton<ISearchContentProvider, SearchContentProvider>();
        services.AddSingleton<ISearchManager, SearchManager>();

        return services;
    }
}