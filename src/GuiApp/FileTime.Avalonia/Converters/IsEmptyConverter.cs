using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FileTime.Avalonia.Converters
{
    public class IsEmptyConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var result = value is string s && string.IsNullOrWhiteSpace(s);
            if (Inverse) result = !result;
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}