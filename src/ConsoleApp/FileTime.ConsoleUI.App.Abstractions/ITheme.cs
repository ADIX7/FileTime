using TerminalUI.Color;

namespace FileTime.ConsoleUI.App;

public interface ITheme
{
    IColor? DefaultForegroundColor { get; }
    IColor? DefaultBackgroundColor { get; }
    IColor? ElementColor { get; }
    IColor? ContainerColor { get; }
    IColor? MarkedItemForegroundColor { get; }
    IColor? MarkedItemBackgroundColor { get; }
    IColor? MarkedSelectedItemForegroundColor { get; }
    IColor? MarkedSelectedItemBackgroundColor { get; }
    IColor? SelectedItemColor { get; }
    IColor? SelectedTabBackgroundColor { get; }
}