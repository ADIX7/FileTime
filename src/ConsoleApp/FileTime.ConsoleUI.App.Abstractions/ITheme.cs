using TerminalUI.Color;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.App;

public interface ITheme
{
    IColor? DefaultForegroundColor { get; }
    IColor? DefaultBackgroundColor { get; }
    IColor? ElementColor { get; }
    IColor? ContainerColor { get; }
    IColor? MarkedItemColor { get; }
}