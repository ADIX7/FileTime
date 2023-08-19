using GeneralInputKey;

namespace FileTime.App.Core.Services;

public interface IKeyInputHandler
{
    Task HandleInputKey(GeneralKeyEventArgs e);
}