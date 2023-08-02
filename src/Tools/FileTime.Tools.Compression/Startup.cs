using FileTime.App.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Tools.Compression;

public class StartupHandler : IStartupHandler
{
    public StartupHandler(IIdentifiableUserCommandService identifiableUserCommandService)
    {
        identifiableUserCommandService.AddIdentifiableUserCommand(CompressUserCommand.Instance);
    }
    public Task InitAsync() => Task.CompletedTask;
}

public static class Startup
{
    public static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddSingleton<IStartupHandler, StartupHandler>();
        services.AddSingleton<CompressCommandFactory>();
        services.AddSingleton<IUserCommandHandler, CompressionUserCommandHandler>();
        return services;
    }
}