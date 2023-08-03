using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.App.Converters;

public class IsTypeConverter : IValueConverter
{
    public bool Invert { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is Type parameterType)
        {
            var result = parameterType.IsInstanceOfType(value);

            if(Invert) result = !result;
            return result;
        }

        throw new NotSupportedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}