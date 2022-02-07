using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.Avalonia.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is DateTime dateTime && parameter is string parameterS
                ? dateTime.ToString(parameterS)
                : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}