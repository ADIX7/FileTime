using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Core.CommandHandlers;

public class StreamCopyCommandHandler : ICommandHandler
{
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public StreamCopyCommandHandler(
        IContentProviderRegistry contentProviderRegistry,
        IContentAccessorFactory contentAccessorFactory)
    {
        _contentProviderRegistry = contentProviderRegistry;
        _contentAccessorFactory = contentAccessorFactory;
    }

    public bool CanHandle(ICommand command)
    {
        if (command is not CopyCommand copyCommand) return false;

        var targetSupportsContentStream =
            _contentProviderRegistry
                .ContentProviders
                .FirstOrDefault(p => p.CanHandlePath(copyCommand.Target!))
                ?.SupportsContentStreams ?? false;

        var allSourcesSupportsContentStream =
            copyCommand
                .Sources
                .Select(s =>
                    _contentProviderRegistry
                        .ContentProviders
                        .FirstOrDefault(p => p.CanHandlePath(s))
                )
                .All(p => p?.SupportsContentStreams ?? false);

        return targetSupportsContentStream && allSourcesSupportsContentStream;
    }

    public async Task ExecuteAsync(ICommand command)
    {
        if (command is not CopyCommand copyCommand) throw new ArgumentException($"Can not execute command of type '{command.GetType()}'.");

        await copyCommand.ExecuteAsync(CopyElement);
    }

    public async Task CopyElement(AbsolutePath sourcePath, AbsolutePath targetPath, CopyCommandContext copyCommandContext)
    {
        var parent = (IContainer?) (await targetPath.GetParent()!.ResolveAsync())!;
        var elementName = targetPath.Path;
        var parentChildren = parent.ItemsCollection.ToList();
        if (parentChildren.All(e => e.Path.GetName() != elementName.GetName()))
        {
            var itemCreator = _contentAccessorFactory.GetItemCreator(parent.Provider);
            await itemCreator.CreateElementAsync(parent.Provider, elementName);
        }

        var source = (IElement?) (await sourcePath.ResolveAsync())!;
        var target = (IElement?) (await targetPath.ResolveAsync())!;

        using var reader = await _contentAccessorFactory.GetContentReaderFactory(source.Provider).CreateContentReaderAsync(source);
        using var writer = await _contentAccessorFactory.GetContentWriterFactory(target.Provider).CreateContentWriterAsync(target);

        byte[] dataRead;
        long currentProgress = 0;

        do
        {
            dataRead = await reader.ReadBytesAsync(writer.PreferredBufferSize);
            if (dataRead.Length > 0)
            {
                await writer.WriteBytesAsync(dataRead);
                await writer.FlushAsync();
                currentProgress += dataRead.LongLength;
                copyCommandContext.CurrentProgress?.SetProgress(currentProgress);
                await copyCommandContext.UpdateProgress();
            }
        } while (dataRead.Length > 0);
    }
}