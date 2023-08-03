using System.Globalization;
using Avalonia.Data.Converters;
using ByteSizeLib;

namespace FileTime.GuiApp.App.Converters;

public class FormatSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value, int.TryParse(parameter?.ToString(), out var prec)) switch
        {
            (long size, true) => ToSizeString(size, prec),
            (long size, false) => ToSizeString(size),
            (null, _) => "...",
            _ => value
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static string ToSizeString(long fileSize, int? precision = null)
    {
        var size = ByteSize.FromBytes(fileSize);
        return precision == null
            ? size.ToString()
            : size.ToString("0." + new string('#', precision.Value));
    }
}