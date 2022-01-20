using FileTime.Core.Models;
using FileTime.Uno.IconProviders;
using FileTime.Uno.ViewModels;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileTime.Uno.Converters
{
    public class ItemToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IIconProvider converter = new MaterialIconProvider();

            IItem item = value switch
            {
                ContainerViewModel container => container.Container,
                ElementViewModel element => element.Element,
                _ => null
            };

            var path = converter.GetImage(item);
            return new SvgImageSource(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
