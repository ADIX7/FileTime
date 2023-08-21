using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.Styling;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.InfoProviders;

public static class ColorSchema
{
    private const int ColorTextMargin = 5;

    public static void PrintColorSchema(IThemeProvider themeProvider, IColorProvider colorProvider, IConsoleDriver consoleDriver)
    {
        var theme = themeProvider.CurrentTheme;

        consoleDriver.Dispose();
        consoleDriver.ResetStyle();
        PrintThemeColors(theme, consoleDriver);

        PrintColorPalette(colorProvider, consoleDriver);
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

        consoleDriver.ResetStyle();
        consoleDriver.Write(Environment.NewLine);
    }

    private static void PrintColorPalette(IColorProvider colorSampleProvider, IConsoleDriver consoleDriver)
    {
        var colorPalette = colorSampleProvider.GetType();

        var colorType = typeof(IColor);
        var colorFields = colorPalette
            .GetFields()
            .Where(f => f.FieldType.IsAssignableTo(colorType) && !f.IsStatic)
            .ToDictionary(f => f.Name, f => (IColor?) f.GetValue(colorSampleProvider));
        var colorProperties = colorPalette
            .GetProperties()
            .Where(p => p.PropertyType.IsAssignableTo(colorType) && p.GetMethod is {IsStatic: false})
            .ToDictionary(p => p.Name, p => (IColor?) p.GetValue(colorSampleProvider));

        var colors = colorFields
            .Concat(colorProperties)
            .OrderBy(v => v.Key)
            .ToDictionary(k => k.Key, v => v.Value);

        consoleDriver.Write("Color palette for " + colorPalette.Name + Environment.NewLine);

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

        consoleDriver.ResetStyle();
        consoleDriver.Write(Environment.NewLine);
    }

    private static void PrintColor(IConsoleDriver consoleDriver, string name, IColor? color, int colorTextStartX)
    {
        consoleDriver.ResetStyle();
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

        consoleDriver.ResetStyle();
        consoleDriver.Write(Environment.NewLine);
    }
}