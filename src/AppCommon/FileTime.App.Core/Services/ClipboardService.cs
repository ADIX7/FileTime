using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public class ClipboardService : IClipboardService
{
    private List<IAbsolutePath> _content;
    public IReadOnlyList<IAbsolutePath> Content { get; private set; }
    public Type? CommandType { get; private set; }

    public ClipboardService()
    {
        _content = new List<IAbsolutePath>();
        Content = _content.AsReadOnly();
    }

    public void AddContent(IAbsolutePath absolutePath)
    {
        foreach (var content in _content)
        {
            if (content.Equals(absolutePath)) return;
        }

        _content.Add(absolutePath);
    }

    public void RemoveContent(IAbsolutePath absolutePath)
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
        _content = new List<IAbsolutePath>();
        Content = _content.AsReadOnly();
        CommandType = null;
    }

    public void SetCommand<T>() where T : ITransportationCommand
    {
        CommandType = typeof(T);
    }
}