using FileTime.Core.Command;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.CommandHandlers;

public static class Startup
{
    public static IServiceCollection AddDefaultCommandHandlers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<ICommandHandler, StreamCopyCommandHandler>();
    }
}