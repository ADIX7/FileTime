using Avalonia;
using Avalonia.Input.Platform;
using FileTime.App.Core.Services;

namespace FileTime.GuiApp.Services;

public class SystemClipboardService : ISystemClipboardService
{
    public async Task CopyToClipboardAsync(string text)
    {
        var clipboard = AvaloniaLocator.Current.GetService<IClipboard>();

        if (clipboard is null) { return; }

        await clipboard.SetTextAsync(text);
    }
    public async Task GetFiles()
    {
        var clipboard = AvaloniaLocator.Current.GetService<IClipboard>();

        if (clipboard is null) { return; }

        await clipboard.ClearAsync();

        var formats = await clipboard.GetFormatsAsync();
        if (!formats.Contains("asd")) return;
        var obj = (await clipboard.GetDataAsync("PNG"));
        /*var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        byte[] data = ms.ToArray().Skip(4).ToArray();
        ms = new MemoryStream(data);*/
        ;
    }
}