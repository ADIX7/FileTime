using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Text;

namespace FileTime.Uno.Models
{
    public class ItemNamePart
    {
        public string Text { get; set; }
        public TextDecorations TextDecorations { get; set; }

        public ItemNamePart(string text)
        {
            Text = text;
        }
    }
}
