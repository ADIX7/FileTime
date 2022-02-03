using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.Avalonia.Converters
{
    public class BoolInverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b ? !b : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}