using FileTime.App.Core.Services;
using FileTime.Core.ContentAccess;
using FileTime.Tools.Compression.Compress;
using FileTime.Tools.Compression.ContentProvider;
using FileTime.Tools.Compression.Decompress;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Tools.Compression;

public class StartupHandler : IStartupHandler
{
    public StartupHandler(IIdentifiableUserCommandService identifiableUserCommandService)
    {
        identifiableUserCommandService.AddIdentifiableUserCommand(CompressUserCommand.Instance);
        identifiableUserCommandService.AddIdentifiableUserCommand(DecompressUserCommand.Instance);
    }
    public Task InitAsync() => Task.CompletedTask;
}

public static class Startup
{
    public static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddSingleton<IStartupHandler, StartupHandler>();
        services.AddSingleton<CompressCommandFactory>();
        services.AddSingleton<DecompressCommandFactory>();
        services.AddSingleton<IUserCommandHandler, CompressionUserCommandHandler>();
        services.TryAddSingleton<ICompressedContentProviderFactory, CompressedContentProviderFactory>();
        services.AddSingleton<ISubContentProvider, CompressedSubContentProvider>();
        services.TryAddSingleton<IContentReaderFactory<CompressedContentProvider>, CompressedContentReaderFactory>();
        return services;
    }
}