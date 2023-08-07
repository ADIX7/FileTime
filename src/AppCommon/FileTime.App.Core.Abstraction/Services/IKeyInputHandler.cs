using FileTime.App.Core.Models;

namespace FileTime.App.Core.Services;

public interface IKeyInputHandler
{
    Task HandleInputKey(Keys key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled);
}