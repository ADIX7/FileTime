using FileTime.App.Database.LiteDb;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Database;

public static class Startup
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseContext, DatabaseContext>();
        services.AddTransient<DatabaseConnection>();
        return services;
    }
}