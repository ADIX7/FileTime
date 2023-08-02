using System.Globalization;
using Avalonia.Data.Converters;
using FileTime.App.ContainerSizeScanner;

namespace FileTime.GuiApp.Converters;

public class ItemSizeToSizeConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [ISizePreviewItem previewItem, ContainerPreview sizeContainerViewModel, double width and > 0])
        {
            var cumulativeSize = sizeContainerViewModel.TopItems.Select(i => i.Size.Value).Sum();
            return width * previewItem.Size.Value / cumulativeSize;
        }

        return null;
    }
}