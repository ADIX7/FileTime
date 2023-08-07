using FileTime.App.Core.Services;
using FileTime.Core.Models;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App.Services;

public class SystemClipboardService : ISystemClipboardService
{
    public Task CopyToClipboardAsync(string text)
    {
        Clipboard.TrySetClipboardData(text);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<FullName>> GetFilesAsync() => throw new NotImplementedException();

    public Task SetFilesAsync(IEnumerable<FullName> files) => throw new NotImplementedException();
}