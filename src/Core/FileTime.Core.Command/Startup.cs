using FileTime.Core.Command.Copy;
using FileTime.Core.Command.Move;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.Command;

public static class Startup
{
    public static IServiceCollection AddCommands(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<CopyCommandFactory>()
            .AddSingleton<MoveCommandFactory>();
}