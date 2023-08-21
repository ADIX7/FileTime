namespace TerminalUI.Color;

public abstract class ColorProviderBase : IColorProvider
{
    public abstract IColor Parse(string color, ColorType type);

    protected IColor? ParseInternal(string color, ColorType type) 
        => (color.ToLower(), type) switch
        {
            ("black", ColorType.Foreground) => BlackForeground,
            ("blue", ColorType.Foreground) => BlueForeground,
            ("cyan", ColorType.Foreground) => CyanForeground,
            ("darkblue", ColorType.Foreground) => DarkBlueForeground,
            ("darkcyan", ColorType.Foreground) => DarkCyanForeground,
            ("darkgray", ColorType.Foreground) => DarkGrayForeground,
            ("darkgreen", ColorType.Foreground) => DarkGreenForeground,
            ("darkmagenta", ColorType.Foreground) => DarkMagentaForeground,
            ("darkred", ColorType.Foreground) => DarkRedForeground,
            ("darkyellow", ColorType.Foreground) => DarkYellowForeground,
            ("gray", ColorType.Foreground) => GrayForeground,
            ("green", ColorType.Foreground) => GreenForeground,
            ("magenta", ColorType.Foreground) => MagentaForeground,
            ("red", ColorType.Foreground) => RedForeground,
            ("white", ColorType.Foreground) => WhiteForeground,
            ("yellow", ColorType.Foreground) => YellowForeground,

            ("black", ColorType.Background) => BlackBackground,
            ("blue", ColorType.Background) => BlueBackground,
            ("cyan", ColorType.Background) => CyanBackground,
            ("darkblue", ColorType.Background) => DarkBlueBackground,
            ("darkcyan", ColorType.Background) => DarkCyanBackground,
            ("darkgray", ColorType.Background) => DarkGrayBackground,
            ("darkgreen", ColorType.Background) => DarkGreenBackground,
            ("darkmagenta", ColorType.Background) => DarkMagentaBackground,
            ("darkred", ColorType.Background) => DarkRedBackground,
            ("darkyellow", ColorType.Background) => DarkYellowBackground,
            ("gray", ColorType.Background) => GrayBackground,
            ("green", ColorType.Background) => GreenBackground,
            ("magenta", ColorType.Background) => MagentaBackground,
            ("red", ColorType.Background) => RedBackground,
            ("white", ColorType.Background) => WhiteBackground,
            ("yellow", ColorType.Background) => YellowBackground,

            _ => null
        };

    public abstract IColor BlackForeground { get; }
    public abstract IColor BlueForeground { get; }
    public abstract IColor CyanForeground { get; }
    public abstract IColor DarkBlueForeground { get; }
    public abstract IColor DarkCyanForeground { get; }
    public abstract IColor DarkGrayForeground { get; }
    public abstract IColor DarkGreenForeground { get; }
    public abstract IColor DarkMagentaForeground { get; }
    public abstract IColor DarkRedForeground { get; }
    public abstract IColor DarkYellowForeground { get; }
    public abstract IColor GrayForeground { get; }
    public abstract IColor GreenForeground { get; }
    public abstract IColor MagentaForeground { get; }
    public abstract IColor RedForeground { get; }
    public abstract IColor WhiteForeground { get; }
    public abstract IColor YellowForeground { get; }

    public abstract IColor BlackBackground { get; }
    public abstract IColor BlueBackground { get; }
    public abstract IColor CyanBackground { get; }
    public abstract IColor DarkBlueBackground { get; }
    public abstract IColor DarkCyanBackground { get; }
    public abstract IColor DarkGrayBackground { get; }
    public abstract IColor DarkGreenBackground { get; }
    public abstract IColor DarkMagentaBackground { get; }
    public abstract IColor DarkRedBackground { get; }
    public abstract IColor DarkYellowBackground { get; }
    public abstract IColor GrayBackground { get; }
    public abstract IColor GreenBackground { get; }
    public abstract IColor MagentaBackground { get; }
    public abstract IColor RedBackground { get; }
    public abstract IColor WhiteBackground { get; }
    public abstract IColor YellowBackground { get; }
}