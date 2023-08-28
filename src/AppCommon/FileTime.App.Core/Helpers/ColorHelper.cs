namespace FileTime.App.Core.Helpers;

public static class ColorHelper
{
    // Convert an HLS value into an RGB value.
    public static (byte r, byte g, byte b) HlsToRgb(double h, double l, double s)
    {
        double p2;
        if (l <= 0.5) p2 = l * (1 + s);
        else p2 = l + s - l * s;

        var p1 = 2 * l - p2;
        double doubleR, doubleG, doubleB;
        if (s == 0)
        {
            doubleR = l;
            doubleG = l;
            doubleB = l;
        }
        else
        {
            doubleR = QqhToRgb(p1, p2, h + 120);
            doubleG = QqhToRgb(p1, p2, h);
            doubleB = QqhToRgb(p1, p2, h - 120);
        }

        // Convert RGB to the 0 to 255 range.
        return ((byte) (doubleR * 255.0),
            (byte) (doubleG * 255.0),
            (byte) (doubleB * 255.0));
    }

    private static double QqhToRgb(double q1, double q2, double hue)
    {
        if (hue > 360) hue -= 360;
        else if (hue < 0) hue += 360;

        if (hue < 60) return q1 + (q2 - q1) * hue / 60;
        if (hue < 180) return q2;
        if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
        return q1;
    }
}