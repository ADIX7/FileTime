namespace FileTime.Core.Interactions
{
    public class InputElement
    {
        public string Label { get; }
        public InputType InputType { get; }
        public string? DefaultValue { get; }
        public List<object>? Options { get; }

        protected InputElement(string text, InputType inputType, string? defaultValue = null)
        {
            Label = text;
            InputType = inputType;
            DefaultValue = defaultValue;
        }

        protected InputElement(string text, InputType inputType, List<object> defaultValue)
        {
            Label = text;
            InputType = inputType;
            Options = defaultValue;
        }

        public static InputElement ForText(string label, string? defaultValue = null)
        {
            return new InputElement(label, InputType.Text, defaultValue);
        }

        public static InputElement ForPassword(string label, string? defaultValue = null)
        {
            return new InputElement(label, InputType.Password, defaultValue);
        }

        public static InputElement ForOptions(string label, List<object> defaultValue)
        {
            return new InputElement(label, InputType.Options, defaultValue);
        }
    }
}