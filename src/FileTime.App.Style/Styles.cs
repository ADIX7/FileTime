using FileTime.ConsoleUI.App.UI.Color;

namespace FileTime.ConsoleUI.App.UI
{
    public class Styles : IStyles
    {
        public IConsoleColor? DefaultBackground { get; }
        public IConsoleColor? DefaultForeground { get; }
        public IConsoleColor? ContainerBackground { get; }
        public IConsoleColor? ContainerForeground { get; }
        public IConsoleColor? ElementBackground { get; }
        public IConsoleColor? ElementForeground { get; }
        public IConsoleColor? ElementSpecialBackground { get; }
        public IConsoleColor? ElementSpecialForeground { get; }
        public IConsoleColor? SelectedItemBackground { get; }
        public IConsoleColor? SelectedItemForeground { get; }

        public IConsoleColor? ErrorColor { get; }
        public IConsoleColor? ErrorInverseColor { get; }
        public IConsoleColor? AccentForeground { get; }

        public Styles(bool useAnsiColors)
        {
            if (useAnsiColors)
            {
                ContainerForeground = AnsiColor.From8bit(4);
                ElementForeground = AnsiColor.From8bit(14);
                ElementSpecialForeground = AnsiColor.From8bit(2);
                SelectedItemForeground = AnsiColor.From8bit(3);
                ElementBackground = AnsiColor.From8bit(0);

                ErrorColor = AnsiColor.From8bit(1);
                ErrorInverseColor = AnsiColor.From8bit(15);
                AccentForeground = AnsiColor.From8bit(2);
            }
            else
            {
                ContainerForeground = new BasicColor(ConsoleColor.Blue);
                ElementBackground = new BasicColor(Console.BackgroundColor);
                ElementForeground = new BasicColor(Console.ForegroundColor);
                ElementSpecialForeground = new BasicColor(ConsoleColor.DarkGreen);
                SelectedItemForeground = new BasicColor(ConsoleColor.DarkYellow);

                ErrorColor = new BasicColor(ConsoleColor.Red);
                ErrorInverseColor = new BasicColor(ConsoleColor.White);
                AccentForeground = new BasicColor(ConsoleColor.Green);
            }

            DefaultForeground = ElementForeground;
            SelectedItemBackground = ElementSpecialBackground = ContainerBackground = DefaultBackground = ElementBackground;
        }
    }
}