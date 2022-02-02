using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.Avalonia.Converters
{
    public class ExceptionToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Exception e) return value;

            if (e is UnauthorizedAccessException) return e.Message;

            return $"{e.Message} ({e.GetType().FullName})";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}