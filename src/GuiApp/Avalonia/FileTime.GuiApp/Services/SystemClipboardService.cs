using System.Net;
using System.Text.Encodings.Web;
using Avalonia.Platform.Storage;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.GuiApp.Services;

public class SystemClipboardService : ISystemClipboardService
{
    private const string ClipboardContentFiles = "Files";
    
    private readonly ITimelessContentProvider _timelessContentProvider;
    public IUiAccessor UiAccessor { get; internal set; }

    public SystemClipboardService(ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
    }

    public async Task CopyToClipboardAsync(string text)
    {
        var clipboard = UiAccessor.GetTopLevel()?.Clipboard;

        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(text);
    }

    public async Task<IEnumerable<FullName>> GetFiles()
    {
        var clipboard = UiAccessor.GetTopLevel()?.Clipboard;

        if (clipboard is null)
        {
            return Enumerable.Empty<FullName>();
        }

        var formats = await UiAccessor.InvokeOnUIThread(async () => await clipboard.GetFormatsAsync());

        if (!formats.Contains(ClipboardContentFiles)) return Enumerable.Empty<FullName>();
        var obj = await clipboard.GetDataAsync(ClipboardContentFiles);

        if (obj is IEnumerable<IStorageItem> storageItems)
        {
            return storageItems
                    .Select(i => _timelessContentProvider.GetFullNameByNativePath(new NativePath(WebUtility.UrlDecode(i.Path.AbsolutePath))))
                    .Where(i => i != null)
                    .OfType<FullName>();
        }
        
        return Enumerable.Empty<FullName>();
    }
}