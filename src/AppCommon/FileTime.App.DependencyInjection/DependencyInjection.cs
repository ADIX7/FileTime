using FileTime.App.Core.Clipboard;
using FileTime.Core.Command;
using FileTime.Core.Providers;
using FileTime.Core.StateManagement;
using FileTime.Providers.Local;
using FileTime.Providers.Smb;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterDefaultServices(IServiceCollection? serviceCollection = null)
        {
            serviceCollection ??= new ServiceCollection();

            return serviceCollection
                .AddSingleton<IClipboard, Clipboard.Clipboard>()
                .AddSingleton<TopContainer>()
                .AddSingleton<LocalContentProvider>()
                .AddSingleton<IContentProvider, LocalContentProvider>(sp => sp.GetService<LocalContentProvider>() ?? throw new Exception($"No {nameof(LocalContentProvider)} instance found"))
                .AddSingleton<IContentProvider, SmbContentProvider>()
                .AddSingleton<ElementCreationStates>()
                .AddSingleton<CommandExecutor>();
        }
    }
}