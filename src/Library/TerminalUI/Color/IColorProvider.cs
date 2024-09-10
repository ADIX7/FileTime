namespace TerminalUI.Color;

public interface IColorProvider
{
    IColor Parse(string color, ColorType type);
    IColor FromRgb(Rgb rgb, ColorType type);

    IColor DefaultForeground { get; }
    IColor BlackForeground { get; }
    IColor BlueForeground { get; }
    IColor CyanForeground { get; }
    IColor DarkBlueForeground { get; }
    IColor DarkCyanForeground { get; }
    IColor DarkGrayForeground { get; }
    IColor DarkGreenForeground { get; }
    IColor DarkMagentaForeground { get; }
    IColor DarkRedForeground { get; }
    IColor DarkYellowForeground { get; }
    IColor GrayForeground { get; }
    IColor GreenForeground { get; }
    IColor MagentaForeground { get; }
    IColor RedForeground { get; }
    IColor WhiteForeground { get; }
    IColor YellowForeground { get; }

    IColor DefaultBackground { get; }
    IColor BlackBackground { get; }
    IColor BlueBackground { get; }
    IColor CyanBackground { get; }
    IColor DarkBlueBackground { get; }
    IColor DarkCyanBackground { get; }
    IColor DarkGrayBackground { get; }
    IColor DarkGreenBackground { get; }
    IColor DarkMagentaBackground { get; }
    IColor DarkRedBackground { get; }
    IColor DarkYellowBackground { get; }
    IColor GrayBackground { get; }
    IColor GreenBackground { get; }
    IColor MagentaBackground { get; }
    IColor RedBackground { get; }
    IColor WhiteBackground { get; }
    IColor YellowBackground { get; }
}
