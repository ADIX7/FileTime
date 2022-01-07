using FileTime.ConsoleUI.App.UI.Color;

namespace FileTime.ConsoleUI.App.UI
{
    public interface IStyles
    {
        IConsoleColor? DefaultBackground { get; }
        IConsoleColor? DefaultForeground { get; }
        IConsoleColor? ContainerBackground { get; }
        IConsoleColor? ContainerForeground { get; }
        IConsoleColor? ElementBackground { get; }
        IConsoleColor? ElementForeground { get; }
        IConsoleColor? ElementSpecialBackground { get; }
        IConsoleColor? ElementSpecialForeground { get; }
        IConsoleColor? SelectedItemBackground { get; }
        IConsoleColor? SelectedItemForeground { get; }
        IConsoleColor? ErrorColor { get; }
        IConsoleColor? ErrorInverseColor { get; }
        IConsoleColor? AccentForeground { get; }
    }
}