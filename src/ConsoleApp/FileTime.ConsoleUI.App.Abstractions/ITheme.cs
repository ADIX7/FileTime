using TerminalUI.Color;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.App;

public interface ITheme
{
    IColor? ItemBackgroundColor { get; }
    IColor? AlternativeItemBackgroundColor { get; }
    IColor? SelectedItemBackgroundColor { get; }
    IColor? MarkedItemBackgroundColor { get; }
    IColor? MarkedAlternativeItemBackgroundColor { get; }
    IColor? MarkedSelectedItemBackgroundColor { get; }
    IColor? DefaultForegroundColor { get; }
    IColor? DefaultBackgroundColor { get; }
    IColor? AlternativeItemForegroundColor { get; }
    IColor? SelectedItemForegroundColor { get; }
    IColor? MarkedItemForegroundColor { get; }
    IColor? MarkedAlternativeItemForegroundColor { get; }
    IColor? MarkedSelectedItemForegroundColor { get; }
}