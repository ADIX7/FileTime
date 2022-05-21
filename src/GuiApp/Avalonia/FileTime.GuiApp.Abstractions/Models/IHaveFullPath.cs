using FileTime.Core.Models;

namespace FileTime.GuiApp.Models;

public interface IHaveFullPath
{
    FullName Path { get; }
}