using System.Net;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.GuiApp.App.Services;

public class SystemClipboardService : ISystemClipboardService
{
    private const string ClipboardContentFiles = "Files";

    private readonly ITimelessContentProvider _timelessContentProvider;
    public IUiAccessor UiAccessor { get; internal set; } = null!;

    public SystemClipboardService(ITimelessContentProvider timelessContentProvider)
        => _timelessContentProvider = timelessContentProvider;

    public async Task CopyToClipboardAsync(string text)
    {
        var clipboard = UiAccessor.GetTopLevel()?.Clipboard;

        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(text);
    }

    public async Task<IEnumerable<FullName>> GetFilesAsync()
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
                .Select(i => _timelessContentProvider.GetFullNameByNativePathAsync(new NativePath(WebUtility.UrlDecode(i.Path.AbsolutePath))))
                .Where(i => i != null)
                .OfType<FullName>();
        }

        return Enumerable.Empty<FullName>();
    }

    public async Task SetFilesAsync(IEnumerable<FullName> files)
    {
        var clipboard = UiAccessor.GetTopLevel()?.Clipboard;

        if (clipboard is null)
        {
            return;
        }

        var topLevel = UiAccessor.GetTopLevel();

        if (topLevel is null)
        {
            //TODO: 
            return;
        }

        var fileNativePaths = files
            .Select(i => _timelessContentProvider.GetNativePathByFullNameAsync(i))
            .Where(i => i != null)
            .OfType<NativePath>();

        var targetFiles = new List<IStorageFile>();
        foreach (var fileNativePath in fileNativePaths)
        {
            var file = await UiAccessor.InvokeOnUIThread(async () => await topLevel.StorageProvider.TryGetFileFromPathAsync(fileNativePath.Path));
            //TODO: Handle null
            if (file != null)
            {
                targetFiles.Add(file);
            }
        }
        
        DataObject dataObject = new();
        dataObject.Set(ClipboardContentFiles, targetFiles);

        await clipboard.SetDataObjectAsync(dataObject);
    }
}