using FileTime.Uno.Application;
using FileTime.Uno.ViewModels;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.Converters
{
    public class ItemViewModeToBrushConverter : IValueConverter
    {
        public string DefaultBrush { get; set; }
        public string SelectedBrush { get; set; }
        public string AlternativeBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ItemViewMode viewMode)
            {
                return viewMode switch
                {
                    ItemViewMode.Default => Microsoft.UI.Xaml.Application.Current.Resources[DefaultBrush],
                    ItemViewMode.Selected => Microsoft.UI.Xaml.Application.Current.Resources[SelectedBrush],
                    ItemViewMode.Alternative => Microsoft.UI.Xaml.Application.Current.Resources[AlternativeBrush],
                    _ => throw new NotSupportedException(),
                };
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
