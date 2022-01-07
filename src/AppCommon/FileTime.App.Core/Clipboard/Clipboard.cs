using FileTime.Core.Command;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Clipboard
{
    public class Clipboard : IClipboard
    {
        private readonly List<ClipboardItem> _content;
        public IReadOnlyList<ClipboardItem> Content { get; }
        public Type? CommandType { get; private set; }

        public Clipboard()
        {
            _content = new List<ClipboardItem>();
            Content = _content.AsReadOnly();
        }

        public void AddContent(IContentProvider contentProvider, string path)
        {
            foreach (var content in _content)
            {
                if (content.ContentProvider == contentProvider && content.Path == path) return;
            }

            _content.Add(new ClipboardItem(contentProvider, path));
        }

        public void RemoveContent(IContentProvider contentProvider, string path)
        {
            for (var i = 0; i < _content.Count; i++)
            {
                if (_content[i].ContentProvider == contentProvider && _content[i].Path == path)
                {
                    _content.RemoveAt(i--);
                }
            }
        }

        public void Clear()
        {
            _content.Clear();
            CommandType = null;
        }

        public void SetCommand<T>() where T : ITransportationCommand
        {
            CommandType = typeof(T);
        }
    }
}