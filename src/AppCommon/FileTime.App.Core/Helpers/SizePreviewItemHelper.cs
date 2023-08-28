using System.Collections.ObjectModel;
using FileTime.App.ContainerSizeScanner;

namespace FileTime.App.Core.Helpers;

public static class SizePreviewItemHelper
{
    public static (byte r, byte g, byte b) GetItemColor(
        ObservableCollection<ISizePreviewItem> items, 
        ISizePreviewItem currentItem,
        double hueDiff = 0,
        double lightness = 0.75
        )
    {
        var i = 0;
        for (; i < items.Count; i++)
        {
            if (items[i].Name == currentItem.Name) break;
        }

        var hue = (360d * i / (items.Count < 1 ? 1 : items.Count)) + hueDiff;
        if (hue > 360) hue -= 360;
        if (hue < 0) hue += 360;

        return ColorHelper.HlsToRgb(hue, lightness, 1);
    }
}