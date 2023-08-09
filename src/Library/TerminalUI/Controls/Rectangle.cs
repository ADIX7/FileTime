using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class Rectangle<T> : View<T>
{
    [Notify] private IColor? _fill;
    public override Size GetRequestedSize() => new(Width ?? 0, Height ?? 0);

    protected override void DefaultRenderer(Position position, Size size)
    {
        var s = new string('█', Width ?? size.Width);
        ApplicationContext?.ConsoleDriver.SetBackgroundColor(Fill ?? new Color.ConsoleColor(System.ConsoleColor.Yellow, ColorType.Background));
        ApplicationContext?.ConsoleDriver.SetForegroundColor(Fill ?? new Color.ConsoleColor(System.ConsoleColor.Yellow, ColorType.Foreground));
        var height = Height ?? size.Height;
        for (var i = 0; i < height; i++)
        {
            ApplicationContext?.ConsoleDriver.SetCursorPosition(position with {Y = position.Y + i});
            ApplicationContext?.ConsoleDriver.Write(s);
        }
    }
}