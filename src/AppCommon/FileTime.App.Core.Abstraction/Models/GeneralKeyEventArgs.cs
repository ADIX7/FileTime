namespace FileTime.App.Core.Models;

public class GeneralKeyEventArgs
{
    private readonly Action<bool> _handledChanged;
    private bool _handled;
    public required Keys Key { get; init; }

    public bool Handled
    {
        get => _handled;
        set
        {
            if (_handled != value)
            {
                _handled = value;
                _handledChanged(value);
            }
        }
    }

    public GeneralKeyEventArgs(Action<bool> handledChanged)
    {
        _handledChanged = handledChanged;
    }
}