﻿using TerminalUI.Color;

namespace FileTime.ConsoleUI.App.Styling;
using IConsoleTheme = TerminalUI.Styling.ITheme; 

public interface ITheme
{
    IColor? DefaultForegroundColor { get; }
    IColor? DefaultForegroundAccentColor { get; }
    IColor? DefaultBackgroundColor { get; }
    IColor? ElementColor { get; }
    IColor? ContainerColor { get; }
    IColor? MarkedItemForegroundColor { get; }
    IColor? MarkedItemBackgroundColor { get; }
    IColor? MarkedSelectedItemForegroundColor { get; }
    IColor? MarkedSelectedItemBackgroundColor { get; }
    IColor? SelectedItemColor { get; }
    IColor? SelectedTabBackgroundColor { get; }
    IColor? WarningForegroundColor { get; }
    IColor? ErrorForegroundColor { get; }
    IConsoleTheme? ConsoleTheme { get; }
    ListViewItemTheme ListViewItemTheme { get; }
}