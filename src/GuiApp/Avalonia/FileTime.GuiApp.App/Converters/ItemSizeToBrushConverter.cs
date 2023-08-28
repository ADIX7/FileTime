using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.Core.Helpers;

namespace FileTime.GuiApp.App.Converters;

public class ItemSizeToBrushConverter : IMultiValueConverter
{
    public double HueDiff { get; set; }
    public double Lightness { get; set; } = 0.75;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [ISizePreviewItem previewItem, ContainerPreview sizeContainerViewModel])
        {
            var (r, g, b) = SizePreviewItemHelper.GetItemColor(
                sizeContainerViewModel.TopItems,
                previewItem,
                HueDiff,
                Lightness
            );

            var task = Dispatcher.UIThread.InvokeAsync(() => new SolidColorBrush(Color.FromRgb(r, g, b)));
            task.Wait();
            return task.Result;
        }

        return null;
    }

    private static byte Normalize(byte b) => (byte) (255 - (255 - b) / 2);
}