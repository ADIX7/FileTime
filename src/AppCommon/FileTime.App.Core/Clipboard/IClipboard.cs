using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Clipboard
{
    public interface IClipboard
    {
        IReadOnlyList<AbsolutePath> Content { get; }
        Type? CommandType { get; }

        void AddContent(AbsolutePath absolutePath);
        void Clear();
        void RemoveContent(AbsolutePath absolutePath);
        void SetCommand<T>() where T : ITransportationCommand;
    }
}