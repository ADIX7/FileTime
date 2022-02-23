using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.ViewModels.ItemPreview;

namespace FileTime.Avalonia.Converters
{
    public class ItemSizeToBrushConverter : IMultiValueConverter
    {
        public double HueDiff { get; set; }
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2
                && values[0] is ISizeItemViewModel sizeContainerViewModel
                && values[1] is IList<ISizeItemViewModel> items)
            {

                int i = 0;
                for (; i < items.Count; i++)
                {
                    if (items[i].Item == sizeContainerViewModel.Item) break;
                }

                var hue = (360d * i / (items.Count < 1 ? 1 : items.Count)) + HueDiff;
                if (hue > 360) hue -= 360;
                if (hue < 0) hue += 360;

                var (r, g, b) = ColorHelper.HlsToRgb(hue, 0.5, 1);
                var task = Dispatcher.UIThread.InvokeAsync(() => new SolidColorBrush(Color.FromRgb(r, g, b)));
                task.Wait();
                return task.Result;
            }

            return null;
        }
    }
}