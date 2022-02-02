using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Avalonia.Converters
{
    public class ItemToImageConverter : IValueConverter
    {
        private readonly IIconProvider _iconProvider;

        public ItemToImageConverter()
        {
            _iconProvider = App.ServiceProvider.GetService<IIconProvider>()!;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IItem item = value switch
            {
                ContainerViewModel container => container.Container,
                ElementViewModel element => element.Element,
                IItem i => i,
                _ => throw new NotImplementedException()
            };

            SvgSource? source;
            var path = _iconProvider.GetImage(item)!;
            if (path.Type == Models.ImagePathType.Absolute)
            {
                source = SvgSource.Load<SvgSource>(path.Path!, null);
            }
            else if(path.Type == Models.ImagePathType.Raw)
            {
                return path.Image;
            }
            else
            {
                source = SvgSource.Load<SvgSource>("avares://FileTime.Avalonia" + path.Path, null);
            }
            return new SvgImage { Source = source };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}