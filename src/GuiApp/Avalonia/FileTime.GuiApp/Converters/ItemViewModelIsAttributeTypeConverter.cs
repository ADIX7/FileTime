using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;

namespace FileTime.GuiApp.Converters;

public class ItemViewModelIsAttributeTypeConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var attributeType = GetAttributeType(value);
        if (parameter == null) return attributeType;
        var result = parameter is ItemAttributeType targetAttribute && attributeType == targetAttribute;
        if (Invert && parameter is ItemAttributeType) result = !result;
        return result;
    }

    private static ItemAttributeType? GetAttributeType(object? value)
    {
        return value switch
        {
            IFileViewModel => ItemAttributeType.File,
            IContainerSizeContainerViewModel => ItemAttributeType.SizeContainer,
            IElementViewModel => ItemAttributeType.Element,
            IContainerViewModel => ItemAttributeType.Container,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}