using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Avalonia.Models
{
    public class ItemNamePart
    {
        public string Text { get; set; }
        public TextDecorationCollection? TextDecorations { get; set; }

        public ItemNamePart(string text)
        {
            Text = text;
        }
    }
}
