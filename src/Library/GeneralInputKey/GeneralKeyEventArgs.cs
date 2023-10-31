namespace GeneralInputKey;

public class GeneralKeyEventArgs(Action<bool>? handledChanged = null)
{
    private bool _handled;
    public required Keys? Key { get; init; }
    public required char KeyChar { get; init; }
    public required SpecialKeysStatus SpecialKeysStatus { get; init; }

    public bool Handled
    {
        get => _handled;
        set
        {
            if (_handled != value)
            {
                _handled = value;
                handledChanged?.Invoke(value);
            }
        }
    }
}