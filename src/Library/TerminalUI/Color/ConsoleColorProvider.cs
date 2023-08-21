namespace TerminalUI.Color;

public class ConsoleColorProvider : ColorProviderBase
{
    public override IColor Parse(string color, ColorType type)
    {
        var finalColor = ParseInternal(color, type);
        if (finalColor is not null) return finalColor;
        
        //TODO get closest color

        throw new NotSupportedException($"Color can not be parsed. {color}");
    }

    public override IColor BlackForeground { get; } = new ConsoleColor(System.ConsoleColor.Black, ColorType.Foreground);
    public override IColor BlueForeground { get; } = new ConsoleColor(System.ConsoleColor.Blue, ColorType.Foreground);
    public override IColor CyanForeground { get; } = new ConsoleColor(System.ConsoleColor.Cyan, ColorType.Foreground);
    public override IColor DarkBlueForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkBlue, ColorType.Foreground);
    public override IColor DarkCyanForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkCyan, ColorType.Foreground);
    public override IColor DarkGrayForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkGray, ColorType.Foreground);
    public override IColor DarkGreenForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkGreen, ColorType.Foreground);
    public override IColor DarkMagentaForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkMagenta, ColorType.Foreground);
    public override IColor DarkRedForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkRed, ColorType.Foreground);
    public override IColor DarkYellowForeground { get; } = new ConsoleColor(System.ConsoleColor.DarkYellow, ColorType.Foreground);
    public override IColor GrayForeground { get; } = new ConsoleColor(System.ConsoleColor.Gray, ColorType.Foreground);
    public override IColor GreenForeground { get; } = new ConsoleColor(System.ConsoleColor.Green, ColorType.Foreground);
    public override IColor MagentaForeground { get; } = new ConsoleColor(System.ConsoleColor.Magenta, ColorType.Foreground);
    public override IColor RedForeground { get; } = new ConsoleColor(System.ConsoleColor.Red, ColorType.Foreground);
    public override IColor WhiteForeground { get; } = new ConsoleColor(System.ConsoleColor.White, ColorType.Foreground);
    public override IColor YellowForeground { get; } = new ConsoleColor(System.ConsoleColor.Yellow, ColorType.Foreground);

    public override IColor BlackBackground => new ConsoleColor(System.ConsoleColor.Black, ColorType.Background);
    public override IColor BlueBackground => new ConsoleColor(System.ConsoleColor.Blue, ColorType.Background);
    public override IColor CyanBackground => new ConsoleColor(System.ConsoleColor.Cyan, ColorType.Background);
    public override IColor DarkBlueBackground => new ConsoleColor(System.ConsoleColor.DarkBlue, ColorType.Background);
    public override IColor DarkCyanBackground => new ConsoleColor(System.ConsoleColor.DarkCyan, ColorType.Background);
    public override IColor DarkGrayBackground => new ConsoleColor(System.ConsoleColor.DarkGray, ColorType.Background);
    public override IColor DarkGreenBackground => new ConsoleColor(System.ConsoleColor.DarkGreen, ColorType.Background);
    public override IColor DarkMagentaBackground => new ConsoleColor(System.ConsoleColor.DarkMagenta, ColorType.Background);
    public override IColor DarkRedBackground => new ConsoleColor(System.ConsoleColor.DarkRed, ColorType.Background);
    public override IColor DarkYellowBackground => new ConsoleColor(System.ConsoleColor.DarkYellow, ColorType.Background);
    public override IColor GrayBackground => new ConsoleColor(System.ConsoleColor.Gray, ColorType.Background);
    public override IColor GreenBackground => new ConsoleColor(System.ConsoleColor.Green, ColorType.Background);
    public override IColor MagentaBackground => new ConsoleColor(System.ConsoleColor.Magenta, ColorType.Background);
    public override IColor RedBackground => new ConsoleColor(System.ConsoleColor.Red, ColorType.Background);
    public override IColor WhiteBackground => new ConsoleColor(System.ConsoleColor.White, ColorType.Background);
    public override IColor YellowBackground => new ConsoleColor(System.ConsoleColor.Yellow, ColorType.Background);
}