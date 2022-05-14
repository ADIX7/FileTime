using FileTime.Core.Models;

namespace FileTime.GuiApp.Models;

public interface IHaveAbsolutePath
{
    IAbsolutePath Path { get; }
}