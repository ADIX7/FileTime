using FileTime.Core.Models;

namespace FileTime.GuiApp.App.Models;

public interface IHaveFullPath
{
    FullName Path { get; }
}