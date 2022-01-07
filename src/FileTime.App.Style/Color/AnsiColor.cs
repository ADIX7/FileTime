namespace FileTime.ConsoleUI.App.UI.Color
{
    public class AnsiColor : IConsoleColor
    {
        public string? Color { get; }
        public string? Prefix { get; }
        public string? Postfix { get; }
        public AnsiColor() { }

        public AnsiColor(string color, string preFix = "5;", string postFix = "m")
        {
            Color = color;
            Prefix = preFix;
            Postfix = postFix;
        }

        public static AnsiColor FromRgb(int r, int g, int b) => new($"{r};{g};{b}", "5;", "m");
        public static AnsiColor From8bit(byte color) => new($"{color}", "5;", "m");

        public override string? ToString()
        {
            return Color == null ? null : Prefix + Color + Postfix;
        }
    }
}