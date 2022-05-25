namespace FileTime.App.Core.Services;

public interface ISystemClipboardService
{
    Task CopyToClipboardAsync(string text);
}