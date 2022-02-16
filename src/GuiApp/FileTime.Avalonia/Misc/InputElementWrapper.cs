using FileTime.Core.Interactions;

namespace FileTime.Avalonia.Misc
{
    public class InputElementWrapper
    {
        public InputElement InputElement { get; }

        public string Value { get; set; }

        public object? Option { get; set; }

        public char? PasswordChar { get; set; }

        public InputElementWrapper(InputElement inputElement, string? defaultValue = null)
        {
            InputElement = inputElement;
            Value = defaultValue ?? "";
            PasswordChar = inputElement.InputType == InputType.Password ? '*' : null;
        }
    }
}
