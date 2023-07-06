using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface ISystemClipboardService
{
    Task CopyToClipboardAsync(string text);
    Task<IEnumerable<FullName>> GetFilesAsync();
    Task SetFilesAsync(IEnumerable<FullName> files);
}