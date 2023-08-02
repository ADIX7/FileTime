using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using FileTime.App.ContainerSizeScanner;
using FileTime.GuiApp.Helper;

namespace FileTime.GuiApp.Converters;

public class ItemSizeToBrushConverter : IMultiValueConverter
{
    public double HueDiff { get; set; }
    public double Lightness { get; set; } = 0.75;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [ISizePreviewItem previewItem, ContainerPreview sizeContainerViewModel])
        {
            var items = sizeContainerViewModel.TopItems;
            var i = 0;
            for (; i < items.Count; i++)
            {
                if (items[i].Name == previewItem.Name) break;
            }

            var hue = (360d * i / (items.Count < 1 ? 1 : items.Count)) + HueDiff;
            if (hue > 360) hue -= 360;
            if (hue < 0) hue += 360;

            var (r, g, b) = ColorHelper.HlsToRgb(hue, Lightness, 1);

            var task = Dispatcher.UIThread.InvokeAsync(() => new SolidColorBrush(Color.FromRgb(r, g, b)));
            task.Wait();
            return task.Result;
        }

        return null;
    }

    private static byte Normalize(byte b) => (byte) (255 - (255 - b) / 2);
}