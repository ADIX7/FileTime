using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.App.Converters;

public class DebugConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}