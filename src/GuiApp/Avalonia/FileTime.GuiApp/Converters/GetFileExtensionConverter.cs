using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.App.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.Converters;

public class GetFileExtensionConverter : IValueConverter
{
    private IItemNameConverterService? _itemNameConverterService;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string fullName) return value;
        _itemNameConverterService ??= DI.ServiceProvider.GetRequiredService<IItemNameConverterService>();

        return _itemNameConverterService.GetFileExtension(fullName);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}