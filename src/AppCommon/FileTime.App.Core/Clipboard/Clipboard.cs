using FileTime.Core.Command;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Clipboard
{
    public class Clipboard : IClipboard
    {
        private readonly List<AbsolutePath> _content;
        public IReadOnlyList<AbsolutePath> Content { get; }
        public Type? CommandType { get; private set; }

        public Clipboard()
        {
            _content = new List<AbsolutePath>();
            Content = _content.AsReadOnly();
        }

        public void AddContent(AbsolutePath absolutePath)
        {
            foreach (var content in _content)
            {
                if (content.IsEqual(absolutePath)) return;
            }

            _content.Add(new AbsolutePath(absolutePath));
        }

        public void RemoveContent(AbsolutePath absolutePath)
        {
            for (var i = 0; i < _content.Count; i++)
            {
                if (_content[i].IsEqual(absolutePath))
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