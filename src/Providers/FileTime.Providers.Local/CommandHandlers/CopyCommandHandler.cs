using FileTime.Core.Command;
using FileTime.Core.Models;
using FileTime.Core.StateManagement;
using FileTime.Core.Timeline;

namespace FileTime.Providers.Local.CommandHandlers
{
    public class CopyCommandHandler : ICommandHandler
    {
        private readonly ElementCreationStates _elementCreationStates;

        public CopyCommandHandler(ElementCreationStates elementCreationStates)
        {
            _elementCreationStates = elementCreationStates;
        }

        public bool CanHandle(object command)
        {
            if (command is not CopyCommand copyCommand) return false;

            if (copyCommand.Target != null && copyCommand.Target is not LocalFolder) return false;

            if (copyCommand.Sources.Any(s => s.ContentProvider is not LocalContentProvider)) return false;

            return true;
        }

        public async Task ExecuteAsync(object command, TimeRunner timeRunner)
        {
            if (command is not CopyCommand copyCommand) throw new ArgumentException($"Can not execute command of type '{command.GetType()}'.");

            await copyCommand.Execute(CopyElement, timeRunner);
        }

        public static void CopyElement(AbsolutePath sourcePath, AbsolutePath targetPath)
        {
            using var sourceStream = File.OpenRead(sourcePath.Path);
            using var sourceReader = new BinaryReader(sourceStream);

            using var targetStream = File.OpenWrite(targetPath.Path);
            using var targetWriter = new BinaryWriter(targetStream);

            const int bufferSize = 1024 * 1024;
            byte[] dataRead;

            do
            {
                dataRead = sourceReader.ReadBytes(bufferSize);
                targetWriter.Write(dataRead);
                targetWriter.Flush();
            }
            while (dataRead.Length > 0);
        }
    }
}