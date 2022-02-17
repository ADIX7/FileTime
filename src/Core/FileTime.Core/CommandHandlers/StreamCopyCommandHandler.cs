using FileTime.Core.Command;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.CommandHandlers
{
    public class StreamCopyCommandHandler : ICommandHandler
    {
        public bool CanHandle(object command)
        {
            if (command is not CopyCommand copyCommand) return false;

            return (copyCommand.Target?.ContentProvider.SupportsContentStreams ?? false)
                    && copyCommand.Sources.All(p => p.ContentProvider.SupportsContentStreams);
        }

        public async Task ExecuteAsync(object command, TimeRunner timeRunner)
        {
            if (command is not CopyCommand copyCommand) throw new ArgumentException($"Can not execute command of type '{command.GetType()}'.");

            await copyCommand.Execute(CopyElement, timeRunner);
        }

        public static async Task CopyElement(AbsolutePath sourcePath, AbsolutePath targetPath, OperationProgress? operationProgress, CopyCommandContext copyCommandContext)
        {
            var parent = (IContainer?)(await targetPath.GetParent().ResolveAsync())!;
            var elementName = targetPath.GetName();
            if (!await parent.IsExistsAsync(elementName))
            {
                await parent.CreateElementAsync(elementName);
            }

            var source = (IElement?)(await sourcePath.ResolveAsync())!;
            var target = (IElement?)(await targetPath.ResolveAsync())!;

            using var reader = await source.GetContentReaderAsync();
            using var writer = await target.GetContentWriterAsync();

            byte[] dataRead;

            do
            {
                dataRead = await reader.ReadBytesAsync(writer.PreferredBufferSize);
                if (dataRead.Length > 0)
                {
                    await writer.WriteBytesAsync(dataRead);
                    await writer.FlushAsync();
                    if (operationProgress != null) operationProgress.Progress += dataRead.LongLength;
                    await copyCommandContext.UpdateProgress();
                }
            }
            while (dataRead.Length > 0);
        }
    }
}