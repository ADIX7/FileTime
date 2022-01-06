namespace FileTime.ConsoleUI.App.UI.Color
{
    public class BasicColor : IConsoleColor
    {
        public BasicColor() { }

        public BasicColor(ConsoleColor color)
        {
            Color = color;
        }

        public ConsoleColor? Color { get; }
    }
}