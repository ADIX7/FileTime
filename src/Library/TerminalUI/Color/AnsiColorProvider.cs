namespace TerminalUI.Color;

public class AnsiColorProvider : ColorProviderBase
{
    public override IColor Parse(string color, ColorType type)
    {
        var finalColor = ParseInternal(color, type);

        finalColor ??= FromRgb(color, type);

        if (finalColor is not null) return finalColor;

        throw new NotSupportedException($"Color can not be parsed. {color}");
    }

    private IColor? FromRgb(string color, ColorType type)
    {
        if (ParseHexColor(color) is var (r, g, b))
        {
            return new ColorRgb(r, g, b, type);
        }

        return null;
    }

    public override IColor FromRgb(Rgb rgb, ColorType type) => new ColorRgb(rgb.R, rgb.G, rgb.B, type);

    public override IColor DefaultForeground { get; } = Color256.DefaultForeground;
    public override IColor BlackForeground { get; } = new Color256(0, ColorType.Foreground);
    public override IColor BlueForeground { get; } = new Color256(12, ColorType.Foreground);
    public override IColor CyanForeground { get; } = new Color256(14, ColorType.Foreground);
    public override IColor DarkBlueForeground { get; } = new Color256(4, ColorType.Foreground);
    public override IColor DarkCyanForeground { get; } = new Color256(6, ColorType.Foreground);
    public override IColor DarkGrayForeground { get; } = new Color256(8, ColorType.Foreground);
    public override IColor DarkGreenForeground { get; } = new Color256(2, ColorType.Foreground);
    public override IColor DarkMagentaForeground { get; } = new Color256(5, ColorType.Foreground);
    public override IColor DarkRedForeground { get; } = new Color256(1, ColorType.Foreground);
    public override IColor DarkYellowForeground { get; } = new Color256(3, ColorType.Foreground);
    public override IColor GrayForeground { get; } = new Color256(7, ColorType.Foreground);
    public override IColor GreenForeground { get; } = new Color256(10, ColorType.Foreground);
    public override IColor MagentaForeground { get; } = new Color256(13, ColorType.Foreground);
    public override IColor RedForeground { get; } = new Color256(9, ColorType.Foreground);
    public override IColor WhiteForeground { get; } = new Color256(15, ColorType.Foreground);
    public override IColor YellowForeground { get; } = new Color256(11, ColorType.Foreground);

    public override IColor DefaultBackground { get; } = Color256.DefaultBackground;
    public override IColor BlackBackground { get; } = new Color256(0, ColorType.Background);
    public override IColor BlueBackground { get; } = new Color256(12, ColorType.Background);
    public override IColor CyanBackground { get; } = new Color256(14, ColorType.Background);
    public override IColor DarkBlueBackground { get; } = new Color256(4, ColorType.Background);
    public override IColor DarkCyanBackground { get; } = new Color256(6, ColorType.Background);
    public override IColor DarkGrayBackground { get; } = new Color256(8, ColorType.Background);
    public override IColor DarkGreenBackground { get; } = new Color256(2, ColorType.Background);
    public override IColor DarkMagentaBackground { get; } = new Color256(5, ColorType.Background);
    public override IColor DarkRedBackground { get; } = new Color256(1, ColorType.Background);
    public override IColor DarkYellowBackground { get; } = new Color256(3, ColorType.Background);
    public override IColor GrayBackground { get; } = new Color256(7, ColorType.Background);
    public override IColor GreenBackground { get; } = new Color256(10, ColorType.Background);
    public override IColor MagentaBackground { get; } = new Color256(13, ColorType.Background);
    public override IColor RedBackground { get; } = new Color256(9, ColorType.Background);
    public override IColor WhiteBackground { get; } = new Color256(15, ColorType.Background);
    public override IColor YellowBackground { get; } = new Color256(11, ColorType.Background);
}
