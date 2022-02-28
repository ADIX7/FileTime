using FileTime.Core.Command;
using FileTime.Core.Providers;
using FileTime.Providers.Favorites.CommandHandlers;
using FileTime.Providers.Favorites.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Favorites
{
    public static class Startup
    {
        public static IServiceCollection AddFavoriteServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<FavoriteContentProvider>()
                .AddSingleton<IContentProvider>(serviceProvider => serviceProvider.GetRequiredService<FavoriteContentProvider>())
                .AddSingleton<PersistenceService>()
                .RegisterFavoriteCommandHandlers();
        }

        internal static IServiceCollection RegisterFavoriteCommandHandlers(this IServiceCollection serviceCollection)
        {
            var commandHandlers = new List<Type>()
            {
                typeof(ToFavoriteCopyCommandHandler)
            };

            foreach (var commandHandler in commandHandlers)
            {
                serviceCollection.AddTransient(typeof(ICommandHandler), commandHandler);
            }

            return serviceCollection;
        }
    }
}