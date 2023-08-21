namespace TerminalUI.Color;

public class AnsiColorProvider : ColorProviderBase
{
    public override IColor Parse(string color, ColorType type)
    {
        var finalColor = ParseInternal(color, type);

        finalColor ??= ParseHexColor(color, type);

        if (finalColor is not null) return finalColor;

        throw new NotSupportedException($"Color can not be parsed. {color}");
    }

    private static IColor? ParseHexColor(string color, ColorType colorType)
    {
        if (!color.StartsWith("#")) return null;
        if (color.Length == 4)
        {
            var r = ColorCharToColorByte(color[1]);
            var g = ColorCharToColorByte(color[2]);
            var b = ColorCharToColorByte(color[3]);

            r += (byte) (r << 4);
            g += (byte) (g << 4);
            b += (byte) (b << 4);

            return new ColorRgb(r, g, b, colorType);
        }
        
        if (color.Length == 7)
        {
            var r = (byte) (ColorCharToColorByte(color[1]) << 4 | ColorCharToColorByte(color[2]));
            var g = (byte) (ColorCharToColorByte(color[3]) << 4 | ColorCharToColorByte(color[4]));
            var b = (byte) (ColorCharToColorByte(color[5]) << 4 | ColorCharToColorByte(color[6]));

            return new ColorRgb(r, g, b, colorType);
        }

        throw new NotSupportedException($"Hex color can not be parsed. {color}");
    }

    private static byte ColorCharToColorByte(char color) =>
        color switch
        {
            >= '0' and <= '9' => (byte) (color - '0'),
            >= 'A' and <= 'F' => (byte) (color - 'A' + 10),
            >= 'a' and <= 'f' => (byte) (color - 'a' + 10),
            _ => throw new NotSupportedException($"Hex color can not be parsed. {color}")
        };

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