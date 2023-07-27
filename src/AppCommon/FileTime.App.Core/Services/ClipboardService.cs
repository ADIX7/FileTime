using System.Collections.ObjectModel;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public class ClipboardService : IClipboardService
{
    private readonly ObservableCollection<FullName> _content;
    public ReadOnlyObservableCollection<FullName> Content { get; }
    public Type? CommandFactoryType { get; private set; }

    public ClipboardService()
    {
        _content = new ObservableCollection<FullName>();
        Content = new(_content);
    }

    public void AddContent(FullName absolutePath)
    {
        foreach (var content in _content)
        {
            if (content.Equals(absolutePath)) return;
        }

        _content.Add(absolutePath);
    }

    public void RemoveContent(FullName absolutePath)
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
        _content.Clear();
        CommandFactoryType = null;
    }

    public void SetCommand<T>() where T : ITransportationCommandFactory
        => CommandFactoryType = typeof(T);
}