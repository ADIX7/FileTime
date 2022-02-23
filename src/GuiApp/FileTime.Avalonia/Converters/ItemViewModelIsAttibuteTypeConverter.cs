using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.ContainerSizeScanner;
using FileTime.Core.Models;
using FileTime.Providers.Local;

namespace FileTime.Avalonia.Converters
{
    public class ItemViewModelIsAttibuteTypeConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var attributeType = GetAttibuteType(value);
            if (parameter == null) return attributeType;
            var result = parameter is AttibuteType targetAttribute && attributeType == targetAttribute;
            if (Invert && parameter is AttibuteType) result = !result;
            return result;
        }

        private static AttibuteType? GetAttibuteType(object? value)
        {
            if (value is ElementViewModel elementVM)
            {
                if (elementVM.BaseElement is IFile)
                {
                    return AttibuteType.File;
                }
                return AttibuteType.Element;
            }
            else if (value is ContainerViewModel containerVM)
            {
                if(containerVM.BaseItem is ContainerSizeContainer) return AttibuteType.SizeContainer;
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