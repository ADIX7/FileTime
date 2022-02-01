using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Models;

namespace FileTime.Avalonia.Converters
{
    public class ItemToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IIconProvider converter = new MaterialIconProvider();

            IItem item = value switch
            {
                ContainerViewModel container => container.Container,
                ElementViewModel element => element.Element,
                _ => throw new NotImplementedException()
            };

            var path = converter.GetImage(item)!;
            var source = SvgSource.Load<SvgSource>("avares://FileTime.Avalonia" + path, null);
            return new SvgImage { Source = source };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}