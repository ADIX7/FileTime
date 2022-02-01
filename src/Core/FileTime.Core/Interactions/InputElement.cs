namespace FileTime.Core.Interactions
{
    public class InputElement
    {
        public string Text { get; }
        public InputType InputType { get; }
        public string? DefaultValue { get; }

        public InputElement(string text, InputType inputType, string? defaultValue = null)
        {
            Text = text;
            InputType = inputType;
            DefaultValue = defaultValue;
        }
    }
}