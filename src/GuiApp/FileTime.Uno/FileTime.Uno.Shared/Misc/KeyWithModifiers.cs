using System;
using System.Collections.Generic;
using System.Text;
using Windows.System;

namespace FileTime.Uno.Misc
{
    public class KeyWithModifiers
    {
        public VirtualKey Key { get; }

        public bool? Alt { get; }
        public bool? Shift { get; }
        public bool? Ctrl { get; }

        public KeyWithModifiers(VirtualKey key, bool alt = false, bool shift = false, bool ctrl = false)
        {
            Key = key;
            Alt = alt;
            Shift = shift;
            Ctrl = ctrl;
        }
    }
}
