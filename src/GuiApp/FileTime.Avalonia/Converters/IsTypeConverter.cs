using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.Avalonia.Converters
{
    public class IsTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            parameter is Type type && type.IsInstanceOfType(value);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}