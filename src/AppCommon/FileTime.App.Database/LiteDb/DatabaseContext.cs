using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Database.LiteDb;

public class DatabaseContext : IDatabaseContext
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public ValueTask<IDatabaseConnection> GetConnectionAsync() 
        => ValueTask.FromResult((IDatabaseConnection)_serviceProvider.GetRequiredService<DatabaseConnection>());
}