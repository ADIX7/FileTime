using System.Diagnostics;

namespace TerminalUI.Color;

[DebuggerDisplay("R = {R}, G = {G}, B = {B}")]
public readonly struct Rgb
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;

    public Rgb(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static double operator -(Rgb left, Rgb right)
    {
        var r = Math.Abs(left.R - right.R);
        var g = Math.Abs(left.G - right.G);
        var b = Math.Abs(left.B - right.B);
        return (double)(r + g + b) / 3;
    }

    public void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }
}