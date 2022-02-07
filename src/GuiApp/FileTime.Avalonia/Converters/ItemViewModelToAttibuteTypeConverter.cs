using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using FileTime.Providers.Local;

namespace FileTime.Avalonia.Converters
{
    public class ItemViewModelIsAttibuteTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return parameter is AttibuteType targetAttribute && GetAttibuteType(value) == targetAttribute;
        }

        private static AttibuteType? GetAttibuteType(object? value)
        {
            if (value is ElementViewModel elementVM)
            {
                if (elementVM.Element is LocalFile)
                {
                    return AttibuteType.LocalFile;
                }
                return AttibuteType.Element;
            }
            else if (value is ContainerViewModel)
            {
                return AttibuteType.Container;
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}