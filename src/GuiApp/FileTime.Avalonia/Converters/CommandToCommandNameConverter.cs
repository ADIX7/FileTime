using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.App.Core.Command;

namespace FileTime.Avalonia.Converters
{
    public class CommandToCommandNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is not Commands command) return value;

            //TODO: implement
            return command.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}