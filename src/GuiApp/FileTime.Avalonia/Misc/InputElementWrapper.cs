using FileTime.Core.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Avalonia.Misc
{
    public class InputElementWrapper
    {

        public InputElement InputElement { get; }

        public string Value { get; set; }

        public InputElementWrapper(InputElement inputElement, string? defaultValue = null)
        {
            InputElement = inputElement;
            Value = defaultValue ?? "";
        }
    }
}
