using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Avalonia.Converters
{
    public class GetFileExtensionConverter : IValueConverter
    {
        private ItemNameConverterService? _itemNameConverterService;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string fullName) return value;
            _itemNameConverterService ??= App.ServiceProvider.GetService<ItemNameConverterService>() ?? throw new Exception($"No {nameof(ItemNameConverterService)} is registered.");;

            return _itemNameConverterService.GetFileExtension(fullName);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}