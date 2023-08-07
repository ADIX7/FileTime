using Avalonia.Input;

namespace FileTime.GuiApp.App.Models;

public class KeyNotSupportedException : Exception
{
    private readonly Key _key;

    public KeyNotSupportedException(Key key) : base($"Key {key} is not supported.")
    {
        _key = key;
    }
}