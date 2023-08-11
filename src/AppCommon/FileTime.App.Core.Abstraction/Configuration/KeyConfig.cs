using FileTime.App.Core.Models;
using GeneralInputKey;

namespace FileTime.App.Core.Configuration;

public class KeyConfig
{
    public Keys Key { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }
    public bool Ctrl { get; set; }

    public KeyConfig() { }

    public KeyConfig(
        Keys key,
        bool shift = false,
        bool alt = false,
        bool ctrl = false)
    {
        Key = key;
        Shift = shift;
        Alt = alt;
        Ctrl = ctrl;
    }

    public bool AreEquals(KeyConfig otherKeyConfig) =>
        Key.Equals(otherKeyConfig.Key)
        && Alt == otherKeyConfig.Alt
        && Shift == otherKeyConfig.Shift
        && Ctrl == otherKeyConfig.Ctrl;
}