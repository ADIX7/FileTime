using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Core.CommandHandlers;

public class StreamCopyCommandHandler : ICommandHandler
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public StreamCopyCommandHandler(IContentAccessorFactory contentAccessorFactory)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public Task<bool> CanHandleAsync(ICommand command) => Task.FromResult(command is CopyCommand);

    public async Task ExecuteAsync(ICommand command)
    {
        if (command is not CopyCommand copyCommand) throw new ArgumentException($"Can not execute command of type '{command.GetType()}'.");

        await copyCommand.ExecuteAsync(CopyElement);
    }

    private async Task CopyElement(AbsolutePath sourcePath, AbsolutePath targetPath, CopyCommandContext copyCommandContext)
    {
        if (copyCommandContext.CancellationToken.IsCancellationRequested) return;

        var parent = (IContainer?) (await targetPath.GetParent()!.ResolveAsync())!;
        var elementName = targetPath.Path;
        var parentChildren = parent.Items.ToList();
        if (parentChildren.All(e => e.Path.GetName() != elementName.GetName()))
        {
            var itemCreator = _contentAccessorFactory.GetItemCreator(parent.Provider);
            await itemCreator.CreateElementAsync(parent.Provider, elementName);
        }

        var source = (IElement?) (await sourcePath.ResolveAsync())!;
        var target = (IElement?) (await targetPath.ResolveAsync())!;

        using var reader = await _contentAccessorFactory.GetContentReaderFactory(source.Provider).CreateContentReaderAsync(source);
        using var writer = await _contentAccessorFactory.GetContentWriterFactory(target.Provider).CreateContentWriterAsync(target);

        var readerStream = reader.GetStream();
        var writerStream = writer.GetStream();

        var dataRead = new byte[1024 * 1024];
        long currentProgress = 0;

        while (true)
        {
            if (copyCommandContext.CancellationToken.IsCancellationRequested) return;
            var readLength = await readerStream.ReadAsync(dataRead);
            var actualDataRead = dataRead[..readLength];
            if (actualDataRead.Length == 0) break;

            await writerStream.WriteAsync(actualDataRead, cancellationToken: copyCommandContext.CancellationToken);
            await writerStream.FlushAsync(copyCommandContext.CancellationToken);
            currentProgress += actualDataRead.LongLength;
            copyCommandContext.CurrentProgress?.SetProgressSafe(currentProgress);

            await copyCommandContext.UpdateProgressAsync();
        }
    }
}