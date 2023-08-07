using FileTime.App.Core.Models;

namespace FileTime.App.Core.Services;

public interface IKeyInputHandler
{
    Task HandleInputKey(GeneralKeyEventArgs e, SpecialKeysStatus specialKeysStatus);
}