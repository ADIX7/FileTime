using FileTime.App.Core.Services;

namespace FileTime.GuiApp.Services;

public class SystemClipboardService : ISystemClipboardService
{
    public async Task CopyToClipboardAsync(string text)
    {
        if (global::Avalonia.Application.Current?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}