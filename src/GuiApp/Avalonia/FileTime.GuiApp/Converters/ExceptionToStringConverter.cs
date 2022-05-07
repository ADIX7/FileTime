using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.Converters;

public class ExceptionToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Exception e) return value;

        if (e is UnauthorizedAccessException)
        {
            return e.Message;
        }
        else if (e.InnerException != null)
        {
            return TraverseInnerException(e);
        }

        return FormatException(e);
    }

    private static string TraverseInnerException(Exception e)
    {
        string s = "";
        if (e.InnerException != null) s += TraverseInnerException(e.InnerException) + Environment.NewLine;
        else return FormatException(e);

        s += "In: " + FormatException(e);

        return s;
    }

    private static string FormatException(Exception e)
    {
        return $"{e.Message} ({e.GetType().FullName})";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}