using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using FileTime.Avalonia.ViewModels.ItemPreview;

namespace FileTime.Avalonia.Converters
{
    public class ItemSizeToSizeConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 3
                && values[0] is ISizeItemViewModel sizeContainerViewModel
                && values[1] is IEnumerable<ISizeItemViewModel> items
                && values[2] is double width && width > 0)
            {
                var commulativeSize = items.Select(i => i.Size).Sum();
                return width * sizeContainerViewModel.Size / commulativeSize;
            }

            return null;
        }
    }
}