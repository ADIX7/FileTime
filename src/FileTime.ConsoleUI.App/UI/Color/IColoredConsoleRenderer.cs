namespace FileTime.ConsoleUI.App.UI.Color
{
    public interface IColoredConsoleRenderer
    {
        IConsoleColor? BackgroundColor { get; set; }
        IConsoleColor? ForegroundColor { get; set; }

        void Clear();
        void ResetColor();
        void Write(string text);
        void Write(char c);
        void Write(string format, params object[] param);
    }
}