using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FileTime.GuiApp.Converters;

public class TextDecorationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b) return TextDecorations.Underline;
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}