using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.Core.Models;

namespace FileTime.Avalonia.Converters
{
    public class IsElementConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is IElement;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}