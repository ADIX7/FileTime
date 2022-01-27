using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FileTime.Avalonia.ViewModels;

namespace FileTime.Avalonia.Converters
{
    public class ItemViewModeToBrushConverter : IValueConverter
    {
        public Brush? DefaultBrush { get; set; }
        public Brush? AlternativeBrush { get; set; }
        public Brush? SelectedBrush { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ItemViewMode viewMode)
            {
                return viewMode switch
                {
                    ItemViewMode.Default => DefaultBrush,
                    ItemViewMode.Alternative => AlternativeBrush,
                    ItemViewMode.Selected => SelectedBrush,
                    _ => throw new NotImplementedException()
                };
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}