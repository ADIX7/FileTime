using Avalonia.Input;

namespace FileTime.Avalonia.Configuration
{
    public class KeyConfig
    {
        public Key Key { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
        public bool Ctrl { get; set; }

        public KeyConfig() { }

        public KeyConfig(Key key, bool shift = false, bool alt = false, bool ctrl = false)
        {
            Key = key;
            Shift = shift;
            Alt = alt;
            Ctrl = ctrl;
        }
    }
}