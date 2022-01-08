namespace FileTime.Core.Interactions
{
    public class InputElement
    {
        public string Text { get; }
        public InputType InputType { get; }

        public InputElement(string text, InputType inputType)
        {
            Text = text;
            InputType = inputType;
        }
    }
}