using FileTime.App.Core.Services;
using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App.Services;

public class ConsoleSystemClipboardService : ISystemClipboardService
{
    public Task CopyToClipboardAsync(string text) => throw new NotImplementedException();

    public Task<IEnumerable<FullName>> GetFilesAsync() => throw new NotImplementedException();

    public Task SetFilesAsync(IEnumerable<FullName> files) => throw new NotImplementedException();
}