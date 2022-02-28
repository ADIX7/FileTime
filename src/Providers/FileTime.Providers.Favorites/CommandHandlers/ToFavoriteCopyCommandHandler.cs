using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.Timeline;

namespace FileTime.Providers.Favorites.CommandHandlers
{
    public class ToFavoriteCopyCommandHandler : ICommandHandler
    {
        public bool CanHandle(object command)
        {
            return command is CopyCommand copyCommand
                && copyCommand.Target?.ContentProvider is FavoriteContentProvider;
        }

        public async Task ExecuteAsync(object command, TimeRunner timeRunner)
        {
            if (command is not CopyCommand copyCommand) throw new ArgumentException($"Command must be {typeof(CopyCommand)}.", nameof(command));
            if (copyCommand.Target is null) throw new NullReferenceException("Command's target can not be null.");

            var resolvedTarget = await copyCommand.Target.ResolveAsync();
            if (resolvedTarget is not FavoriteContainerBase targetContainer) throw new Exception($"Target is not {nameof(FavoriteContainerBase)}.");

            foreach (var source in copyCommand.Sources)
            {
                var resolvedSource = await source.ResolveAsync();
                if (resolvedSource == null) continue;

                var newElement = new FavoriteElement(targetContainer, resolvedSource.Name, resolvedSource);
                await targetContainer.AddElementAsync(newElement);
            }
        }
    }
}