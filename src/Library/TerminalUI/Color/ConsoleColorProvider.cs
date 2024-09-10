// ReSharper disable InconsistentNaming
namespace TerminalUI.Color;

public class ConsoleColorProvider : ColorProviderBase
{
    private static readonly List<(Rgb, IColor)> Foregrounds;
    private static readonly List<(Rgb, IColor)> Backgrounds;

    static ConsoleColorProvider()
    {
        Foregrounds = new List<(Rgb, IColor)>
        {
            (new Rgb(0, 0, 0), _blackForeground),
            (new Rgb(0, 0, 128), _darkBlueForeground),
            (new Rgb(0, 128, 0), _darkGreenForeground),
            (new Rgb(0, 128, 128), _darkCyanForeground),
            (new Rgb(128, 0, 0), _darkRedForeground),
            (new Rgb(128, 0, 128), _darkMagentaForeground),
            (new Rgb(128, 128, 0), _darkYellowForeground),
            (new Rgb(192, 192, 192), _grayForeground),
            (new Rgb(128, 128, 128), _darkGrayForeground),
            (new Rgb(0, 0, 255), _blueForeground),
            (new Rgb(0, 255, 0), _greenForeground),
            (new Rgb(0, 255, 255), _cyanForeground),
            (new Rgb(255, 0, 0), _redForeground),
            (new Rgb(255, 0, 255), _magentaForeground),
            (new Rgb(255, 255, 0), _yellowForeground),
            (new Rgb(255, 255, 255), _whiteForeground),
        };

        Backgrounds = new List<(Rgb, IColor)>
        {
            (new Rgb(0, 0, 0), _blackBackground),
            (new Rgb(0, 0, 128), _darkBlueBackground),
            (new Rgb(0, 128, 0), _darkGreenBackground),
            (new Rgb(0, 128, 128), _darkCyanBackground),
            (new Rgb(128, 0, 0), _darkRedBackground),
            (new Rgb(128, 0, 128), _darkMagentaBackground),
            (new Rgb(128, 128, 0), _darkYellowBackground),
            (new Rgb(192, 192, 192), _grayBackground),
            (new Rgb(128, 128, 128), _darkGrayBackground),
            (new Rgb(0, 0, 255), _blueBackground),
            (new Rgb(0, 255, 0), _greenBackground),
            (new Rgb(0, 255, 255), _cyanBackground),
            (new Rgb(255, 0, 0), _redBackground),
            (new Rgb(255, 0, 255), _magentaBackground),
            (new Rgb(255, 255, 0), _yellowBackground),
            (new Rgb(255, 255, 255), _whiteBackground),
        };
    }

    public override IColor Parse(string color, ColorType type)
    {
        var finalColor = ParseInternal(color, type);
        if (finalColor is not null) return finalColor;

        //TODO get closest color
        var rgb = ParseHexColor(color);

        if (rgb is null)
        {
            throw new NotSupportedException($"Color can not be parsed. {color}");
        }

        return FromRgb(rgb.Value, type);
    }

    public override IColor FromRgb(Rgb rgb, ColorType type)
        => type == ColorType.Background
            ? ApproximateColor(rgb, Backgrounds)
            : ApproximateColor(rgb, Foregrounds);

    protected IColor ApproximateColor(Rgb rgb, IEnumerable<(Rgb, IColor)> colors)
        => colors.MinBy(c => c.Item1 - rgb).Item2;

    public override IColor DefaultForeground => _blackForeground;
    public override IColor BlackForeground => _blackForeground;
    public override IColor BlueForeground => _blueForeground;
    public override IColor CyanForeground => _cyanForeground;
    public override IColor DarkBlueForeground => _darkBlueForeground;
    public override IColor DarkCyanForeground => _darkCyanForeground;
    public override IColor DarkGrayForeground => _darkGrayForeground;
    public override IColor DarkGreenForeground => _darkGreenForeground;
    public override IColor DarkMagentaForeground => _darkMagentaForeground;
    public override IColor DarkRedForeground => _darkRedForeground;
    public override IColor DarkYellowForeground => _darkYellowForeground;
    public override IColor GrayForeground => _grayForeground;
    public override IColor GreenForeground => _greenForeground;
    public override IColor MagentaForeground => _magentaForeground;
    public override IColor RedForeground => _redForeground;
    public override IColor WhiteForeground => _whiteForeground;
    public override IColor YellowForeground => _yellowForeground;

    public override IColor DefaultBackground => _blackBackground;
    public override IColor BlackBackground => _blackBackground;
    public override IColor BlueBackground => _blueBackground;
    public override IColor CyanBackground => _cyanBackground;
    public override IColor DarkBlueBackground => _darkBlueBackground;
    public override IColor DarkCyanBackground => _darkCyanBackground;
    public override IColor DarkGrayBackground => _darkGrayBackground;
    public override IColor DarkGreenBackground => _darkGreenBackground;
    public override IColor DarkMagentaBackground => _darkMagentaBackground;
    public override IColor DarkRedBackground => _darkRedBackground;
    public override IColor DarkYellowBackground => _darkYellowBackground;
    public override IColor GrayBackground => _grayBackground;
    public override IColor GreenBackground => _greenBackground;
    public override IColor MagentaBackground => _magentaBackground;
    public override IColor RedBackground => _redBackground;
    public override IColor WhiteBackground => _whiteBackground;
    public override IColor YellowBackground => _yellowBackground;

    private static readonly IColor _blackForeground = new ConsoleColor(System.ConsoleColor.Black, ColorType.Foreground);
    private static readonly IColor _blueForeground = new ConsoleColor(System.ConsoleColor.Blue, ColorType.Foreground);
    private static readonly IColor _cyanForeground = new ConsoleColor(System.ConsoleColor.Cyan, ColorType.Foreground);
    private static readonly IColor _darkBlueForeground = new ConsoleColor(System.ConsoleColor.DarkBlue, ColorType.Foreground);
    private static readonly IColor _darkCyanForeground = new ConsoleColor(System.ConsoleColor.DarkCyan, ColorType.Foreground);
    private static readonly IColor _darkGrayForeground = new ConsoleColor(System.ConsoleColor.DarkGray, ColorType.Foreground);
    private static readonly IColor _darkGreenForeground = new ConsoleColor(System.ConsoleColor.DarkGreen, ColorType.Foreground);
    private static readonly IColor _darkMagentaForeground = new ConsoleColor(System.ConsoleColor.DarkMagenta, ColorType.Foreground);
    private static readonly IColor _darkRedForeground = new ConsoleColor(System.ConsoleColor.DarkRed, ColorType.Foreground);
    private static readonly IColor _darkYellowForeground = new ConsoleColor(System.ConsoleColor.DarkYellow, ColorType.Foreground);
    private static readonly IColor _grayForeground = new ConsoleColor(System.ConsoleColor.Gray, ColorType.Foreground);
    private static readonly IColor _greenForeground = new ConsoleColor(System.ConsoleColor.Green, ColorType.Foreground);
    private static readonly IColor _magentaForeground = new ConsoleColor(System.ConsoleColor.Magenta, ColorType.Foreground);
    private static readonly IColor _redForeground = new ConsoleColor(System.ConsoleColor.Red, ColorType.Foreground);
    private static readonly IColor _whiteForeground = new ConsoleColor(System.ConsoleColor.White, ColorType.Foreground);
    private static readonly IColor _yellowForeground = new ConsoleColor(System.ConsoleColor.Yellow, ColorType.Foreground);

    private static readonly IColor _blackBackground = new ConsoleColor(System.ConsoleColor.Black, ColorType.Background);
    private static readonly IColor _blueBackground = new ConsoleColor(System.ConsoleColor.Blue, ColorType.Background);
    private static readonly IColor _cyanBackground = new ConsoleColor(System.ConsoleColor.Cyan, ColorType.Background);
    private static readonly IColor _darkBlueBackground = new ConsoleColor(System.ConsoleColor.DarkBlue, ColorType.Background);
    private static readonly IColor _darkCyanBackground = new ConsoleColor(System.ConsoleColor.DarkCyan, ColorType.Background);
    private static readonly IColor _darkGrayBackground = new ConsoleColor(System.ConsoleColor.DarkGray, ColorType.Background);
    private static readonly IColor _darkGreenBackground = new ConsoleColor(System.ConsoleColor.DarkGreen, ColorType.Background);
    private static readonly IColor _darkMagentaBackground = new ConsoleColor(System.ConsoleColor.DarkMagenta, ColorType.Background);
    private static readonly IColor _darkRedBackground = new ConsoleColor(System.ConsoleColor.DarkRed, ColorType.Background);
    private static readonly IColor _darkYellowBackground = new ConsoleColor(System.ConsoleColor.DarkYellow, ColorType.Background);
    private static readonly IColor _grayBackground = new ConsoleColor(System.ConsoleColor.Gray, ColorType.Background);
    private static readonly IColor _greenBackground = new ConsoleColor(System.ConsoleColor.Green, ColorType.Background);
    private static readonly IColor _magentaBackground = new ConsoleColor(System.ConsoleColor.Magenta, ColorType.Background);
    private static readonly IColor _redBackground = new ConsoleColor(System.ConsoleColor.Red, ColorType.Background);
    private static readonly IColor _whiteBackground = new ConsoleColor(System.ConsoleColor.White, ColorType.Background);
    private static readonly IColor _yellowBackground = new ConsoleColor(System.ConsoleColor.Yellow, ColorType.Background);
}
