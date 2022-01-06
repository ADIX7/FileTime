using FileTime.Core.Command;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Clipboard
{
    public interface IClipboard
    {
        IReadOnlyList<ClipboardItem> Content { get; }
        Type? CommandType { get; }

        void AddContent(IContentProvider contentProvider, string path);
        void Clear();
        void RemoveContent(IContentProvider contentProvider, string path);
        void SetCommand<T>() where T : ITransportationCommand;
    }
}