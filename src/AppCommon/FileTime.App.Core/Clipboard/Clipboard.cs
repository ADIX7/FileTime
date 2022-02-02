using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Clipboard
{
    public class Clipboard : IClipboard
    {
        private List<AbsolutePath> _content;
        public IReadOnlyList<AbsolutePath> Content { get; private set; }
        public Type? CommandType { get; private set; }

        public Clipboard()
        {
            ResetContent();
        }

        public void AddContent(AbsolutePath absolutePath)
        {
            foreach (var content in _content)
            {
                if (content.Equals(absolutePath)) return;
            }

            _content.Add(new AbsolutePath(absolutePath));
        }

        public void RemoveContent(AbsolutePath absolutePath)
        {
            for (var i = 0; i < _content.Count; i++)
            {
                if (_content[i].Equals(absolutePath))
                {
                    _content.RemoveAt(i--);
                }
            }
        }

        public void Clear()
        {
            ResetContent();
            CommandType = null;
        }

        public void SetCommand<T>() where T : ITransportationCommand
        {
            CommandType = typeof(T);
        }

        private void ResetContent()
        {
            _content = new List<AbsolutePath>();
            Content = _content.AsReadOnly();
        }
    }
}