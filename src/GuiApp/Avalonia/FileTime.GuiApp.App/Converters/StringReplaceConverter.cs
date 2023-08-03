using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.App.Converters;

public class StringReplaceConverter : IValueConverter
{
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && OldValue != null && NewValue != null ? s.Replace(OldValue, NewValue) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}