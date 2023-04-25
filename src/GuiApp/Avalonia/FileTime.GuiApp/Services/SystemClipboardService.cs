using Avalonia;
using Avalonia.Input.Platform;
using FileTime.App.Core.Services;

namespace FileTime.GuiApp.Services;

public class SystemClipboardService : ISystemClipboardService
{
    public async Task CopyToClipboardAsync(string text)
    {
        var clipboard = AvaloniaLocator.Current.GetService<IClipboard>();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}