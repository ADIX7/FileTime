using TerminalUI.Color;

namespace FileTime.ConsoleUI.App.Styling;

public record ListViewItemTheme(
    IColor? SelectedBackgroundColor,
    IColor? SelectedForegroundColor
);