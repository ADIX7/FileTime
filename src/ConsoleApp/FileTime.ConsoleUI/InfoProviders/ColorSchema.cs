using FileTime.ConsoleUI.App;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.InfoProviders;

public static class ColorSchema
{
    private const int ColorTextMargin = 5;

    public static void PrintColorSchema(ITheme theme, IConsoleDriver consoleDriver)
    {
        consoleDriver.Dispose();
        consoleDriver.ResetColor();
        PrintThemeColors(theme, consoleDriver);

        if (theme is IColorSampleProvider colorSampleProvider)
            PrintColorPalette(colorSampleProvider, consoleDriver);
    }

    private static void PrintThemeColors(ITheme theme, IConsoleDriver consoleDriver)
    {
        consoleDriver.Write("Theme colors:" + Environment.NewLine);

        var colorType = typeof(IColor);
        var colorProperties = typeof(ITheme)
            .GetProperties()
            .Where(p => p.PropertyType.IsAssignableTo(colorType))
            .OrderBy(p => p.Name)
            .ToList();

        if (colorProperties.Count == 0)
        {
            consoleDriver.Write("No colors properties found");
            return;
        }

        var colorTextStartX = colorProperties.Max(p => p.Name.Length) + ColorTextMargin;

        foreach (var colorProperty in colorProperties)
        {
            var color = colorProperty.GetValue(theme) as IColor;

            PrintColor(consoleDriver, colorProperty.Name, color, colorTextStartX);
        }

        consoleDriver.ResetColor();
        consoleDriver.Write(Environment.NewLine);
    }

    private static void PrintColorPalette(IColorSampleProvider colorSampleProvider, IConsoleDriver consoleDriver)
    {
        if (colorSampleProvider.ForegroundColors is { } foregroundColors)
        {
            PrintColorPalette("foreground", foregroundColors, consoleDriver);
        }

        if (colorSampleProvider.BackgroundColors is { } backgroundColors)
        {
            PrintColorPalette("background", backgroundColors, consoleDriver);
        }
    }

    private static void PrintColorPalette(string paletteName, Type colorPalette, IConsoleDriver consoleDriver)
    {
        var colorType = typeof(IColor);
        var colorFields = colorPalette
            .GetFields()
            .Where(f => f.FieldType.IsAssignableTo(colorType) && f.IsStatic)
            .ToDictionary(f => f.Name, f => (IColor?) f.GetValue(null));
        var colorProperties = colorPalette
            .GetProperties()
            .Where(p => p.PropertyType.IsAssignableTo(colorType) && (p.GetMethod?.IsStatic ?? false))
            .ToDictionary(p => p.Name, p => (IColor?) p.GetValue(null));

        var colors = colorFields
            .Concat(colorProperties)
            .OrderBy(v => v.Key)
            .ToDictionary(k => k.Key, v => v.Value);

        consoleDriver.Write("Color palette for " + paletteName + Environment.NewLine);

        if (colors.Count == 0)
        {
            consoleDriver.Write("No colors found");
            consoleDriver.Write(Environment.NewLine);
            return;
        }

        var colorTextStartX = colors.Max(p => p.Key.Length) + ColorTextMargin;
        foreach (var (key, value) in colors)
        {
            PrintColor(consoleDriver, key, value, colorTextStartX);
        }
        
        consoleDriver.ResetColor();
        consoleDriver.Write(Environment.NewLine);
    }

    private static void PrintColor(IConsoleDriver consoleDriver, string name, IColor? color, int colorTextStartX)
    {
        consoleDriver.ResetColor();
        consoleDriver.Write(name + ":");
        var y = consoleDriver.GetCursorPosition().Y;
        consoleDriver.SetCursorPosition(new Position(colorTextStartX, y));

        if (color is null)
        {
            consoleDriver.Write("<null>");
        }
        else
        {
            if (color.Type == ColorType.Foreground)
            {
                consoleDriver.SetForegroundColor(color);
            }
            else
            {
                consoleDriver.SetBackgroundColor(color);
            }

            consoleDriver.Write("Sample text");
        }

        consoleDriver.ResetColor();
        consoleDriver.Write(Environment.NewLine);
    }
}