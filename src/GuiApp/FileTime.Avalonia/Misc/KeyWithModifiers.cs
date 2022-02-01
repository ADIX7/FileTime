using Avalonia.Input;

namespace FileTime.Avalonia.Misc
{
    public class KeyWithModifiers
    {
        public Key Key { get; }

        public bool? Alt { get; }
        public bool? Shift { get; }
        public bool? Ctrl { get; }

        public KeyWithModifiers(Key key, bool alt = false, bool shift = false, bool ctrl = false)
        {
            Key = key;
            Alt = alt;
            Shift = shift;
            Ctrl = ctrl;
        }
    }
}
